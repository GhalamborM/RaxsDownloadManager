using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RD.Core.Interfaces;
using RD.Core.Models;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace RD.Core.Services;
#pragma warning disable
public partial class DownloadManager : IDownloadManager, IDisposable
{
    private readonly IDownloadInfoProvider _downloadInfoProvider;
    private readonly ISegmentDownloader _segmentDownloader;
    private readonly IFileMerger _fileMerger;
    private readonly ILogger<DownloadManager> _logger;
    private readonly DownloadManagerOptions _options;

    private readonly ConcurrentDictionary<string, DownloadTask> _activeDownloads = new();
    private readonly SemaphoreSlim _downloadSemaphore;
    private readonly Timer _progressTimer;
    private readonly object _eventLock = new();
    private bool _disposed;

    public event EventHandler<DownloadProgress>? ProgressChanged;
    public event EventHandler<DownloadProgress>? DownloadCompleted;
    public event EventHandler<DownloadProgress>? DownloadFailed;

    public DownloadManager(
        IDownloadInfoProvider downloadInfoProvider,
        ISegmentDownloader segmentDownloader,
        IFileMerger fileMerger,
        IOptions<DownloadManagerOptions> options,
        ILogger<DownloadManager> logger)
    {
        _downloadInfoProvider = downloadInfoProvider;
        _segmentDownloader = segmentDownloader;
        _fileMerger = fileMerger;
        _logger = logger;
        _options = options.Value;

        _downloadSemaphore = new SemaphoreSlim(_options.MaxConcurrentDownloads, _options.MaxConcurrentDownloads);
        _progressTimer = new Timer(ReportProgress, null, _options.ProgressReportInterval, _options.ProgressReportInterval);
    }

    public async Task<string> StartDownloadAsync(DownloadOptions options, CancellationToken cancellationToken = default)
    {
        ValidateDownloadOptions(options);

        var downloadId = Guid.NewGuid().ToString("N");
        var downloadTask = new DownloadTask
        {
            Id = downloadId,
            Options = options,
            Progress = new DownloadProgress
            {
                DownloadId = downloadId,
                FileName = Path.GetFileName(options.FilePath) ?? "Unknown",
                Status = DownloadStatus.Pending
            },
            CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken),
            PauseCancellationTokenSource = new CancellationTokenSource(),
            Stopwatch = new Stopwatch()
        };

        _activeDownloads[downloadId] = downloadTask;

        // Start the download task
        _ = Task.Run(async () =>
        {
            await _downloadSemaphore.WaitAsync(downloadTask.CancellationTokenSource.Token);
            try
            {
                await ExecuteDownloadAsync(downloadTask);
            }
            finally
            {
                _downloadSemaphore.Release();
            }
        }, downloadTask.CancellationTokenSource.Token);

        _logger.LogInformation("Started download {DownloadId} for URL: {Url}", downloadId, options.Url);
        return downloadId;
    }

    public async Task<string> RestoreDownloadAsync(string downloadId, DownloadOptions options, DownloadProgress savedProgress, CancellationToken cancellationToken = default)
    {
        ValidateDownloadOptions(options);

        if (string.IsNullOrEmpty(downloadId))
        {
            downloadId = Guid.NewGuid().ToString("N");
        }

        var downloadTask = new DownloadTask
        {
            Id = downloadId,
            Options = options,
            Progress = new DownloadProgress
            {
                DownloadId = downloadId,
                FileName = savedProgress.FileName,
                Status = DownloadStatus.Paused,
                TotalBytes = savedProgress.TotalBytes,
                DownloadedBytes = savedProgress.DownloadedBytes,
                BytesPerSecond = 0, // Reset speed on restore
                ElapsedTime = savedProgress.ElapsedTime,
                EstimatedTimeRemaining = null, // Will be recalculated
                ActiveSegments = 0, // Will be updated when resumed
                LastUpdated = DateTime.UtcNow,
                ErrorMessage = savedProgress.ErrorMessage
            },
            CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken),
            PauseCancellationTokenSource = new CancellationTokenSource(),
            Stopwatch = new Stopwatch(),
            IsPaused = true,
            ExecutionTask = null // Will be set when starting the execution task
        };

        _activeDownloads[downloadId] = downloadTask;

        // Start the download execution task but in paused state
        downloadTask.ExecutionTask = Task.Run(async () =>
        {
            await _downloadSemaphore.WaitAsync(downloadTask.CancellationTokenSource.Token);
            try
            {
                await ExecuteDownloadAsync(downloadTask);
            }
            finally
            {
                _downloadSemaphore.Release();
            }
        }, downloadTask.CancellationTokenSource.Token);

        _logger.LogInformation("Restored paused download {DownloadId} for URL: {Url}", downloadId, options.Url);
        
        return downloadId;
    }

    public async Task<bool> PauseDownloadAsync(string downloadId)
    {
        if (_activeDownloads.TryGetValue(downloadId, out var downloadTask))
        {
            lock (downloadTask.StateLock)
            {
                downloadTask.IsPaused = true;
                downloadTask.Progress.Status = DownloadStatus.Paused;
                
                // Cancel the current download operations to pause them
                downloadTask.PauseCancellationTokenSource?.Cancel();
                downloadTask.PauseCancellationTokenSource?.Dispose();
                downloadTask.PauseCancellationTokenSource = new CancellationTokenSource();
                
                // Pause the stopwatch to track accurate elapsed time
                downloadTask.Stopwatch.Stop();
                
                // Force immediate progress update to reflect paused state
                OnProgressChanged(downloadTask.Progress);
            }
            
            _logger.LogInformation("Paused download {DownloadId}", downloadId);
            return true;
        }
        return false;
    }

    public async Task<bool> ResumeDownloadAsync(string downloadId)
    {
        if (_activeDownloads.TryGetValue(downloadId, out var downloadTask))
        {
            lock (downloadTask.StateLock)
            {
                downloadTask.IsPaused = false;
                if (downloadTask.Progress.Status == DownloadStatus.Paused)
                {
                    downloadTask.Progress.Status = DownloadStatus.Downloading;
                    
                    // Resume the stopwatch
                    downloadTask.Stopwatch.Start();
                    
                    // Force immediate progress update to reflect resumed state
                    OnProgressChanged(downloadTask.Progress);
                }
            }
            
            _logger.LogInformation("Resumed download {DownloadId}", downloadId);
            return true;
        }
        return false;
    }

    public async Task<bool> CancelDownloadAsync(string downloadId)
    {
        if (_activeDownloads.TryGetValue(downloadId, out var downloadTask))
        {
            downloadTask.CancellationTokenSource.Cancel();
            downloadTask.Progress.Status = DownloadStatus.Cancelled;
            _logger.LogInformation("Cancelled download {DownloadId}", downloadId);
            return true;
        }
        return false;
    }

    public DownloadProgress? GetDownloadProgress(string downloadId)
    {
        return _activeDownloads.TryGetValue(downloadId, out var downloadTask) ? downloadTask.Progress : null;
    }

    public IEnumerable<DownloadProgress> GetAllDownloadProgress()
    {
        return [.. _activeDownloads.Values.Select(d => d.Progress)];
    }

    public IEnumerable<DownloadSegment>? GetDownloadSegments(string downloadId)
    {
        if (_activeDownloads.TryGetValue(downloadId, out var downloadTask))
        {
            return downloadTask.Segments?.ToList();
        }
        return null;
    }

    private async Task ExecuteDownloadAsync(DownloadTask downloadTask)
    {
        try
        {
            // Wait while paused at the start (for restored downloads)
            while (downloadTask.IsPaused && !downloadTask.CancellationTokenSource.Token.IsCancellationRequested)
            {
                await Task.Delay(100, downloadTask.CancellationTokenSource.Token);
            }

            if (downloadTask.CancellationTokenSource.Token.IsCancellationRequested)
            {
                downloadTask.Progress.Status = DownloadStatus.Cancelled;
                return;
            }

            downloadTask.Stopwatch.Start();
            downloadTask.Progress.Status = DownloadStatus.Downloading;

            // Get download information
            var downloadInfo = await _downloadInfoProvider.GetDownloadInfoAsync(
                downloadTask.Options.Url, downloadTask.Options, downloadTask.CancellationTokenSource.Token);

            // Only update total bytes if not already set (for restored downloads)
            if (downloadTask.Progress.TotalBytes == 0)
            {
                downloadTask.Progress.TotalBytes = downloadInfo.ContentLength;
            }
            
            if (!string.IsNullOrEmpty(downloadInfo.FileName) && 
                string.IsNullOrEmpty(Path.GetFileName(downloadTask.Options.FilePath)))
            {
                downloadTask.Options.FilePath = Path.Combine(
                    Path.GetDirectoryName(downloadTask.Options.FilePath) ?? string.Empty,
                    downloadInfo.FileName);
                downloadTask.Progress.FileName = downloadInfo.FileName;
            }

            // Create directory if it doesn't exist
            var directory = Path.GetDirectoryName(downloadTask.Options.FilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Check if file already exists and handle accordingly
            if (File.Exists(downloadTask.Options.FilePath) && !downloadTask.Options.OverwriteExisting)
            {
                if (!downloadTask.Options.EnableResume)
                {
                    throw new InvalidOperationException($"File already exists: {downloadTask.Options.FilePath}");
                }
            }

            // Determine if we should use segments
            var useSegments = downloadInfo.SupportsPartialContent && 
                            downloadInfo.ContentLength > 0 && 
                            downloadTask.Options.MaxSegments > 1;

            if (useSegments)
            {
                await DownloadWithSegmentsAsync(downloadTask, downloadInfo);
            }
            else
            {
                await DownloadSingleStreamAsync(downloadTask, downloadInfo);
            }

            downloadTask.Progress.Status = DownloadStatus.Completed;
            OnDownloadCompleted(downloadTask.Progress);

            _logger.LogInformation("Download {DownloadId} completed successfully", downloadTask.Id);
        }
        catch (OperationCanceledException) when (downloadTask.CancellationTokenSource.Token.IsCancellationRequested)
        {
            downloadTask.Progress.Status = DownloadStatus.Cancelled;
            _logger.LogInformation("Download {DownloadId} was cancelled", downloadTask.Id);
        }
        catch (Exception ex)
        {
            downloadTask.Progress.Status = DownloadStatus.Failed;
            downloadTask.Progress.ErrorMessage = ex.Message;
            OnDownloadFailed(downloadTask.Progress);
            
            _logger.LogError(ex, "Download {DownloadId} failed", downloadTask.Id);
        }
        finally
        {
            downloadTask.Stopwatch.Stop();
            CleanupDownload(downloadTask);
        }
    }

    private async Task DownloadWithSegmentsAsync(DownloadTask downloadTask, DownloadInfo downloadInfo)
    {
        // Check if we're resuming and segments already exist
        if (downloadTask.Segments == null || downloadTask.Segments.Count == 0)
        {
            // Create new segments
            var segmentSize = downloadInfo.ContentLength / downloadTask.Options.MaxSegments;
            var segments = new List<DownloadSegment>();

            for (int i = 0; i < downloadTask.Options.MaxSegments; i++)
            {
                var startByte = i * segmentSize;
                var endByte = i == downloadTask.Options.MaxSegments - 1 ? 
                    downloadInfo.ContentLength - 1 : 
                    (startByte + segmentSize - 1);

                var segment = new DownloadSegment
                {
                    Id = i,
                    DownloadId = downloadTask.Id,
                    StartByte = startByte,
                    EndByte = endByte,
                    TempFilePath = Path.Combine(_options.TempDirectory, $"{downloadTask.Id}_segment_{i}.tmp"),
                    Status = DownloadStatus.Pending
                };

                // Check if segment file exists and adjust start byte for resume
                if (File.Exists(segment.TempFilePath))
                {
                    var existingFileInfo = new FileInfo(segment.TempFilePath);
                    segment.DownloadedBytes = existingFileInfo.Length;
                    segment.StartByte += existingFileInfo.Length;
                    
                    if (segment.StartByte >= segment.EndByte)
                    {
                        segment.Status = DownloadStatus.Completed;
                    }
                }

                segments.Add(segment);
            }

            downloadTask.Segments = segments;
        }

        downloadTask.Progress.ActiveSegments = downloadTask.Segments.Count(s => s.Status != DownloadStatus.Completed);

        // Download segments with pause/resume support
        await DownloadSegmentsWithPauseSupport(downloadTask, downloadTask.Segments);

        // Merge segments
        downloadTask.Progress.Status = DownloadStatus.Merging;
        var segmentPaths = downloadTask.Segments.OrderBy(s => s.Id).Select(s => s.TempFilePath).ToList();
        
        await _fileMerger.MergeSegmentsAsync(segmentPaths, downloadTask.Options.FilePath, 
            downloadTask.CancellationTokenSource.Token);

        // Cleanup temp files if configured
        if (_options.CleanupTempFilesOnSuccess)
        {
            foreach (var segmentPath in segmentPaths)
            {
                try
                {
                    if (File.Exists(segmentPath))
                    {
                        File.Delete(segmentPath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete temp file: {TempFile}", segmentPath);
                }
            }
        }
    }

    private async Task DownloadSingleStreamAsync(DownloadTask downloadTask, DownloadInfo downloadInfo)
    {
        long startByte = 0;
        
        // Check if we're resuming a download
        if (File.Exists(downloadTask.Options.FilePath) && downloadTask.Options.EnableResume)
        {
            var existingFileInfo = new FileInfo(downloadTask.Options.FilePath);
            startByte = existingFileInfo.Length;
            downloadTask.Progress.DownloadedBytes = startByte;
        }

        var segment = new DownloadSegment
        {
            Id = 0,
            DownloadId = downloadTask.Id,
            StartByte = startByte,
            EndByte = downloadInfo.ContentLength - 1,
            TempFilePath = downloadTask.Options.FilePath,
            Status = DownloadStatus.Downloading,
            DownloadedBytes = startByte
        };

        downloadTask.Segments = [segment];
        downloadTask.Progress.ActiveSegments = 1;

        await DownloadSegmentWithPauseSupport(downloadTask, segment);
    }

    private async Task DownloadSegmentsWithPauseSupport(DownloadTask downloadTask, List<DownloadSegment> segments)
    {
        var segmentTasks = new List<Task>();
        
        foreach (var segment in segments)
        {
            var segmentTask = Task.Run(async () =>
            {
                await DownloadSegmentWithPauseSupport(downloadTask, segment);
            });
            segmentTasks.Add(segmentTask);
        }
        
        await Task.WhenAll(segmentTasks);
    }

    private async Task DownloadSegmentWithPauseSupport(DownloadTask downloadTask, DownloadSegment segment)
    {
        var progress = new Progress<long>(bytesRead =>
        {
            Interlocked.Add(ref downloadTask.TotalBytesDownloaded, bytesRead);
        });

        while (!downloadTask.CancellationTokenSource.Token.IsCancellationRequested && segment.Status != DownloadStatus.Completed)
        {
            // Wait while paused
            while (downloadTask.IsPaused && !downloadTask.CancellationTokenSource.Token.IsCancellationRequested)
            {
                // Ensure segment status reflects paused state
                if (segment.Status == DownloadStatus.Downloading)
                {
                    segment.Status = DownloadStatus.Paused;
                }
                
                await Task.Delay(100, downloadTask.CancellationTokenSource.Token);
            }

            if (downloadTask.CancellationTokenSource.Token.IsCancellationRequested)
            {
                break;
            }

            try
            {
                segment.Status = DownloadStatus.Downloading;
                
                // Create a combined cancellation token that cancels when either the main download is cancelled or when paused
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    downloadTask.CancellationTokenSource.Token,
                    downloadTask.PauseCancellationTokenSource?.Token ?? CancellationToken.None);

                await _segmentDownloader.DownloadSegmentAsync(segment, downloadTask.Options, 
                    progress, combinedCts.Token);
                
                segment.Status = DownloadStatus.Completed;
                break;
            }
            catch (OperationCanceledException) when (downloadTask.PauseCancellationTokenSource?.Token.IsCancellationRequested == true)
            {
                // Download was paused, continue the loop to wait for resume
                segment.Status = DownloadStatus.Paused;
                _logger.LogDebug("Segment {SegmentId} paused", segment.Id);
            }
            catch (OperationCanceledException) when (downloadTask.CancellationTokenSource.Token.IsCancellationRequested)
            {
                // Download was cancelled
                segment.Status = DownloadStatus.Cancelled;
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Segment {SegmentId} failed, will retry", segment.Id);
                segment.Status = DownloadStatus.Pending;
                await Task.Delay(downloadTask.Options.RetryDelay, downloadTask.CancellationTokenSource.Token);
            }
        }
    }

    private void ReportProgress(object? state)
    {
        foreach (var downloadTask in _activeDownloads.Values)
        {
            // Don't update progress for paused downloads to prevent status override
            lock (downloadTask.StateLock)
            {
                if (downloadTask.IsPaused && downloadTask.Progress.Status == DownloadStatus.Paused)
                {
                    // Still report progress but preserve paused status
                    var pausedProgress = downloadTask.Progress;
                    UpdateDownloadProgressData(downloadTask);
                    pausedProgress.Status = DownloadStatus.Paused; // Ensure status remains paused
                    OnProgressChanged(pausedProgress);
                }
                else if (!downloadTask.IsPaused)
                {
                    UpdateDownloadProgress(downloadTask);
                    OnProgressChanged(downloadTask.Progress);
                }
            }
        }
    }

    private void UpdateDownloadProgress(DownloadTask downloadTask)
    {
        UpdateDownloadProgressData(downloadTask);
    }

    private void UpdateDownloadProgressData(DownloadTask downloadTask)
    {
        var progress = downloadTask.Progress;
        var currentBytes = downloadTask.Segments?.Sum(s => s.DownloadedBytes) ?? downloadTask.TotalBytesDownloaded;
        
        if (currentBytes > progress.DownloadedBytes)
        {
            var bytesPerSecond = downloadTask.Stopwatch.ElapsedMilliseconds > 0 
                ? (long)(currentBytes / (downloadTask.Stopwatch.ElapsedMilliseconds / 1000.0))
                : 0;

            progress.DownloadedBytes = currentBytes;
            progress.BytesPerSecond = bytesPerSecond;
            progress.ElapsedTime = downloadTask.Stopwatch.Elapsed;

            if (bytesPerSecond > 0 && progress.TotalBytes > 0)
            {
                var remainingBytes = progress.TotalBytes - progress.DownloadedBytes;
                progress.EstimatedTimeRemaining = TimeSpan.FromSeconds(remainingBytes / bytesPerSecond);
            }

            progress.LastUpdated = DateTime.UtcNow;
        }

        // Update active segments count only for non-paused downloads
        if (!downloadTask.IsPaused)
        {
            progress.ActiveSegments = downloadTask.Segments?.Count(s => s.Status == DownloadStatus.Downloading) ?? 0;
        }
    }

    private void CleanupDownload(DownloadTask downloadTask)
    {
        if (_options.CleanupTempFilesOnFailure && downloadTask.Progress.Status == DownloadStatus.Failed)
        {
            if (downloadTask.Segments != null)
            {
                foreach (var segment in downloadTask.Segments)
                {
                    try
                    {
                        if (File.Exists(segment.TempFilePath))
                        {
                            File.Delete(segment.TempFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to cleanup temp file: {TempFile}", segment.TempFilePath);
                    }
                }
            }
        }

        _activeDownloads.TryRemove(downloadTask.Id, out _);
        downloadTask.CancellationTokenSource.Dispose();
        downloadTask.PauseCancellationTokenSource?.Dispose();
    }

    private void ValidateDownloadOptions(DownloadOptions options)
    {
        if (string.IsNullOrEmpty(options.Url))
            throw new ArgumentException("URL cannot be null or empty", nameof(options.Url));
        
        if (string.IsNullOrEmpty(options.FilePath))
            throw new ArgumentException("FilePath cannot be null or empty", nameof(options.FilePath));
        
        if (options.MaxSegments < 1 || options.MaxSegments > 16)
            throw new ArgumentException("MaxSegments must be between 1 and 16", nameof(options.MaxSegments));
        
        if (!Uri.IsWellFormedUriString(options.Url, UriKind.Absolute))
            throw new ArgumentException("Invalid URL format", nameof(options.Url));
    }

    protected virtual void OnProgressChanged(DownloadProgress progress)
    {
        lock (_eventLock)
        {
            ProgressChanged?.Invoke(this, progress);
        }
    }

    protected virtual void OnDownloadCompleted(DownloadProgress progress)
    {
        lock (_eventLock)
        {
            DownloadCompleted?.Invoke(this, progress);
        }
    }

    protected virtual void OnDownloadFailed(DownloadProgress progress)
    {
        lock (_eventLock)
        {
            DownloadFailed?.Invoke(this, progress);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _progressTimer?.Dispose();
        _downloadSemaphore?.Dispose();

        foreach (var downloadTask in _activeDownloads.Values)
        {
            downloadTask.CancellationTokenSource.Cancel();
            downloadTask.CancellationTokenSource.Dispose();
            downloadTask.PauseCancellationTokenSource?.Dispose();
        }

        _activeDownloads.Clear();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
#pragma warning restore