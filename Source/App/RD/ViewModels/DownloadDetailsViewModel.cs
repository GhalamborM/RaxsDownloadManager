using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RD.Core.Interfaces;
using RD.Core.Models;
using System.Collections.ObjectModel;

namespace RD.ViewModels;

public partial class DownloadDetailsViewModel(IDownloadManager downloadManager) : ObservableObject
{
    private readonly IDownloadManager _downloadManager = downloadManager;
    private DownloadItem? _downloadItem;
    private System.Threading.Timer? _updateTimer;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _url = string.Empty;

    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private double _overallProgress;

    [ObservableProperty]
    private string _sizeText = string.Empty;

    [ObservableProperty]
    private string _speedText = string.Empty;

    [ObservableProperty]
    private string _elapsedTime = string.Empty;

    [ObservableProperty]
    private string _estimatedTimeRemaining = string.Empty;

    [ObservableProperty]
    private ObservableCollection<DownloadSegmentItem> _segments = [];

    [ObservableProperty]
    private bool _canPause;

    [ObservableProperty]
    private bool _canResume;

    public void SetDownload(DownloadItem downloadItem)
    {
        _downloadItem = downloadItem;
        UpdateDisplay();
        StartPeriodicUpdates();
    }

    [RelayCommand]
    private async Task PauseDownloadAsync()
    {
        if (_downloadItem != null && CanPause)
        {
            await _downloadManager.PauseDownloadAsync(_downloadItem.DownloadId);
        }
    }

    [RelayCommand]
    private async Task ResumeDownloadAsync()
    {
        if (_downloadItem != null && CanResume)
        {
            await _downloadManager.ResumeDownloadAsync(_downloadItem.DownloadId);
        }
    }

    [RelayCommand]
    private async Task RestartDownloadAsync()
    {
        if (_downloadItem != null)
        {
            await _downloadManager.CancelDownloadAsync(_downloadItem.DownloadId);
            
            var options = new DownloadOptions
            {
                Url = _downloadItem.Url,
                FilePath = _downloadItem.FilePath
            };

            var downloadId = await _downloadManager.StartDownloadAsync(options);
            _downloadItem.DownloadId = downloadId;
            UpdateDisplay();
        }
    }

    private void StartPeriodicUpdates()
    {
        _updateTimer?.Dispose();
        _updateTimer = new System.Threading.Timer(async _ => await UpdateSegmentsAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    private void UpdateDisplay()
    {
        if (_downloadItem == null) return;

        FileName = _downloadItem.FileName;
        Url = _downloadItem.Url;
        FilePath = _downloadItem.FilePath;
        Status = _downloadItem.StatusText;
        OverallProgress = IsCompleted() ? 100 : _downloadItem.PercentageComplete;
        SizeText = _downloadItem.SizeText;
        SpeedText = IsCompleted() ? "" : _downloadItem.SpeedText;
        ElapsedTime = IsCompleted() ? "" : FormatTimeSpan(_downloadItem.ElapsedTime);
        EstimatedTimeRemaining = IsCompleted() ? "" : _downloadItem.EstimatedTimeRemaining?.ToString(@"hh\:mm\:ss") ?? "Unknown";

        CanPause = _downloadItem.Status == DownloadStatus.Downloading;
        CanResume = _downloadItem.Status == DownloadStatus.Paused;

        bool IsCompleted() => _downloadItem!.Status == DownloadStatus.Completed;

    }

    private async Task UpdateSegmentsAsync()
    {
        if (_downloadItem == null) return;

        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
        {
            // Update main download info
            var progress = _downloadManager.GetDownloadProgress(_downloadItem.DownloadId);
            if (progress != null)
            {
                _downloadItem.UpdateFromProgress(progress);
                UpdateDisplay();
            }

            // Get real-time segment information from the download manager
            var segments = _downloadManager.GetDownloadSegments(_downloadItem.DownloadId);
            if (segments != null && segments.Any())
            {
                UpdateSegmentsFromDownloadManager(segments);
            }
            else if (Segments.Count == 0 && _downloadItem.Status != DownloadStatus.Pending)
            {
                // Fallback to dummy segments if download manager doesn't have segments yet
                CreateDummySegments();
            }
        });
    }

    private void UpdateSegmentsFromDownloadManager(IEnumerable<DownloadSegment> segments)
    {
        var segmentList = segments.ToList();
        
        foreach (var segment in segmentList)
        {
            var existingSegment = Segments.FirstOrDefault(s => s.Id == segment.Id);
            if (existingSegment != null)
            {
                existingSegment.UpdateFromSegment(segment);
            }
            else
            {
                var segmentItem = new DownloadSegmentItem();
                segmentItem.UpdateFromSegment(segment);
                Segments.Add(segmentItem);
            }
        }

        // Remove segments that no longer exist
        var segmentsToRemove = Segments.Where(s => !segmentList.Any(seg => seg.Id == s.Id)).ToList();
        foreach (var segment in segmentsToRemove)
        {
            Segments.Remove(segment);
        }
    }

    private void CreateDummySegments()
    {
        if (_downloadItem == null || _downloadItem.TotalBytes <= 0) return;

        var segmentCount = Math.Min(8, _downloadItem.ActiveSegments > 0 ? _downloadItem.ActiveSegments : 4);
        var segmentSize = _downloadItem.TotalBytes / segmentCount;

        Segments.Clear();
        for (int i = 0; i < segmentCount; i++)
        {
            var startByte = i * segmentSize;
            var endByte = i == segmentCount - 1 ? _downloadItem.TotalBytes - 1 : (i + 1) * segmentSize - 1;
            var downloadedBytes = (long)(_downloadItem.PercentageComplete / 100.0 * (endByte - startByte + 1));

            var segment = new DownloadSegmentItem
            {
                Id = i,
                DownloadId = _downloadItem.DownloadId,
                StartByte = startByte,
                EndByte = endByte,
                DownloadedBytes = downloadedBytes,
                Status = _downloadItem.Status == DownloadStatus.Completed ? DownloadStatus.Completed :
                         _downloadItem.Status == DownloadStatus.Downloading ? DownloadStatus.Downloading :
                         DownloadStatus.Paused
            };

            Segments.Add(segment);
        }
    }

    private static string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalDays >= 1)
            return $"{(int)timeSpan.TotalDays}d {timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        else
            return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }

    public void Cleanup()
    {
        _updateTimer?.Dispose();
    }
}