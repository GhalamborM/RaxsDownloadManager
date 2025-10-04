using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RD.Core.Interfaces;
using RD.Core.Models;
using Scheduler.Events;
using Scheduler.Interfaces;
using Scheduler.Models;
using System.Collections.Concurrent;

namespace Scheduler.Services;

public class DownloadTaskScheduler : ITaskScheduler
{
    private readonly IDownloadManager _downloadManager;
    private readonly ILogger<DownloadTaskScheduler> _logger;
    private readonly SchedulerOptions _options;
    private readonly ConcurrentDictionary<string, ScheduledTask> _tasks = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _taskSemaphores = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _taskCancellationTokens = new();
    private readonly Timer _schedulerTimer;
    private readonly object _eventLock = new();
    private bool _disposed;

    public bool IsGloballyEnabled { get; set; } = true;
    public IReadOnlyList<ScheduledTask> Tasks => _tasks.Values.ToList().AsReadOnly();

    // Events
    public event EventHandler<TaskEventArgs>? TaskStarted;
    public event EventHandler<TaskEventArgs>? TaskCompleted;
    public event EventHandler<TaskEventArgs>? TaskStopped;
    public event EventHandler<TaskErrorEventArgs>? TaskFailed;
    public event EventHandler<DownloadEventArgs>? DownloadStarted;
    public event EventHandler<DownloadEventArgs>? DownloadCompleted;
    public event EventHandler<DownloadErrorEventArgs>? DownloadFailed;

    public DownloadTaskScheduler(
        IDownloadManager downloadManager,
        IOptions<SchedulerOptions> options,
        ILogger<DownloadTaskScheduler> logger)
    {
        _downloadManager = downloadManager;
        _logger = logger;
        _options = options.Value;
        IsGloballyEnabled = _options.IsGloballyEnabled;

        // Subscribe to download manager events
        _downloadManager.DownloadCompleted += OnDownloadManagerDownloadCompleted;
        _downloadManager.DownloadFailed += OnDownloadManagerDownloadFailed;

        // Start the scheduler timer
        _schedulerTimer = new Timer(CheckScheduledTasks, null, TimeSpan.Zero, _options.CheckInterval);

        _logger.LogInformation("Download Task Scheduler initialized with check interval: {Interval}", _options.CheckInterval);
    }

    public ScheduledTask CreateTask(string? name = null, DateTime? startTime = null, DateTime? endTime = null, int? maxConcurrentDownloads = null)
    {
        var task = new ScheduledTask
        {
            Name = name,
            StartTime = startTime ?? DateTime.Now,
            EndTime = endTime,
            MaxConcurrentDownloads = maxConcurrentDownloads ?? _options.DefaultMaxConcurrentDownloads
        };

        _tasks[task.Id] = task;
        _taskSemaphores[task.Id] = new SemaphoreSlim(task.MaxConcurrentDownloads ?? _options.DefaultMaxConcurrentDownloads);
        _taskCancellationTokens[task.Id] = new CancellationTokenSource();

        _logger.LogInformation("Created task {TaskId} ({TaskName}) scheduled for {StartTime}", 
            task.Id, task.Name ?? "Unnamed", task.StartTime);

        return task;
    }

    public ScheduledDownload AddDownloadToTask(string taskId, string url, string filePath, DownloadOptions? options = null)
    {
        if (!_tasks.TryGetValue(taskId, out var task))
            throw new ArgumentException($"Task with ID {taskId} not found", nameof(taskId));

        var download = new ScheduledDownload
        {
            Url = url,
            FilePath = filePath,
            Options = options ?? new DownloadOptions { Url = url, FilePath = filePath }
        };

        task.Downloads.Add(download);

        _logger.LogInformation("Added download {DownloadId} to task {TaskId}", download.Id, taskId);

        return download;
    }

    public bool RemoveDownloadFromTask(string taskId, string downloadId)
    {
        if (!_tasks.TryGetValue(taskId, out var task))
            return false;

        var download = task.Downloads.FirstOrDefault(d => d.Id == downloadId);
        if (download == null)
            return false;

        // Cancel the download if it's running
        if (download.Status == ScheduledDownloadStatus.Running && !string.IsNullOrEmpty(download.DownloadId))
        {
            _ = _downloadManager.CancelDownloadAsync(download.DownloadId);
        }

        download.Status = ScheduledDownloadStatus.Removed;
        task.Downloads.Remove(download);

        _logger.LogInformation("Removed download {DownloadId} from task {TaskId}", downloadId, taskId);

        return true;
    }

    public bool MoveDownload(string fromTaskId, string toTaskId, string downloadId)
    {
        if (!_tasks.TryGetValue(fromTaskId, out var fromTask) || 
            !_tasks.TryGetValue(toTaskId, out var toTask))
            return false;

        var download = fromTask.Downloads.FirstOrDefault(d => d.Id == downloadId);
        if (download == null)
            return false;

        // Only allow moving if the download is not currently running
        if (download.Status == ScheduledDownloadStatus.Running)
            return false;

        fromTask.Downloads.Remove(download);
        toTask.Downloads.Add(download);

        _logger.LogInformation("Moved download {DownloadId} from task {FromTaskId} to task {ToTaskId}", 
            downloadId, fromTaskId, toTaskId);

        return true;
    }

    public bool SetTaskEnabled(string taskId, bool enabled)
    {
        if (!_tasks.TryGetValue(taskId, out var task))
            return false;

        task.IsEnabled = enabled;

        _logger.LogInformation("Task {TaskId} {Status}", taskId, enabled ? "enabled" : "disabled");

        return true;
    }

    public async Task<bool> StartTaskAsync(string taskId)
    {
        if (!_tasks.TryGetValue(taskId, out var task))
            return false;

        if (task.IsRunning)
            return true; // Already running

        task.IsEnabled = true;
        await ExecuteTaskAsync(task);

        return true;
    }

    public async Task<bool> StopTaskAsync(string taskId)
    {
        if (!_tasks.TryGetValue(taskId, out var task))
            return false;

        if (_taskCancellationTokens.TryGetValue(taskId, out var cts))
        {
            cts.Cancel();
        }

        task.IsRunning = false;
        task.Status = ScheduledTaskStatus.Stopped;

        // Cancel all running downloads in this task
        var runningDownloads = task.Downloads.Where(d => d.Status == ScheduledDownloadStatus.Running).ToList();
        foreach (var download in runningDownloads)
        {
            if (!string.IsNullOrEmpty(download.DownloadId))
            {
                await _downloadManager.CancelDownloadAsync(download.DownloadId);
                download.Status = ScheduledDownloadStatus.Cancelled;
            }
        }

        OnTaskStopped(new TaskEventArgs(task));

        _logger.LogInformation("Stopped task {TaskId}", taskId);

        return true;
    }

    public async Task<bool> RemoveTaskAsync(string taskId)
    {
        if (!_tasks.TryGetValue(taskId, out _))
            return false;

        await StopTaskAsync(taskId);

        _tasks.TryRemove(taskId, out _);
        if (_taskSemaphores.TryRemove(taskId, out var semaphore))
        {
            semaphore.Dispose();
        }
        if (_taskCancellationTokens.TryRemove(taskId, out var cts))
        {
            cts.Dispose();
        }

        _logger.LogInformation("Removed task {TaskId}", taskId);

        return true;
    }

    public ScheduledTask? GetTask(string taskId)
    {
        return _tasks.TryGetValue(taskId, out var task) ? task : null;
    }

    public bool UpdateTask(string taskId, string? name = null, DateTime? startTime = null, DateTime? endTime = null, int? maxConcurrentDownloads = null)
    {
        if (!_tasks.TryGetValue(taskId, out var task))
            return false;

        if (name != null)
            task.Name = name;

        if (startTime.HasValue)
            task.StartTime = startTime.Value;

        if (endTime.HasValue)
            task.EndTime = endTime.Value;

        if (maxConcurrentDownloads.HasValue)
        {
            task.MaxConcurrentDownloads = maxConcurrentDownloads.Value;
            
            // Update semaphore if task is not running
            if (!task.IsRunning && _taskSemaphores.TryGetValue(taskId, out var semaphore))
            {
                semaphore.Dispose();
                _taskSemaphores[taskId] = new SemaphoreSlim(maxConcurrentDownloads.Value);
            }
        }

        _logger.LogInformation("Updated task {TaskId}", taskId);

        return true;
    }

    private async void CheckScheduledTasks(object? state)
    {
        if (!IsGloballyEnabled || _disposed)
            return;

        try
        {
            var tasksToExecute = _tasks.Values
                .Where(t => t.ShouldRun && !t.IsRunning)
                .ToList();

            var tasksToStop = _tasks.Values
                .Where(t => t.IsRunning && t.ShouldStop)
                .ToList();

            // Stop tasks that have reached their end time
            foreach (var task in tasksToStop)
            {
                await StopTaskAsync(task.Id);
            }

            // Start eligible tasks
            foreach (var task in tasksToExecute)
            {
                _ = Task.Run(async () => await ExecuteTaskAsync(task));
            }

            // Clean up completed tasks if configured
            if (_options.AutoRemoveCompletedTasks)
            {
                var completedTasks = _tasks.Values
                    .Where(t => t.Status == ScheduledTaskStatus.Completed && 
                               (DateTime.Now - (t.LastRunAt ?? DateTime.Now)) > TimeSpan.FromHours(1))
                    .ToList();

                foreach (var task in completedTasks)
                {
                    await RemoveTaskAsync(task.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking scheduled tasks");
        }
    }

    private async Task ExecuteTaskAsync(ScheduledTask task)
    {
        if (!_taskCancellationTokens.TryGetValue(task.Id, out var cts) || cts.Token.IsCancellationRequested)
            return;

        if (!_taskSemaphores.TryGetValue(task.Id, out var semaphore))
            return;

        task.IsRunning = true;
        task.Status = ScheduledTaskStatus.Running;
        task.LastRunAt = DateTime.Now;

        OnTaskStarted(new TaskEventArgs(task));

        _logger.LogInformation("Started executing task {TaskId} ({TaskName})", task.Id, task.Name ?? "Unnamed");

        try
        {
            var scheduledDownloads = task.Downloads
                .Where(d => d.Status == ScheduledDownloadStatus.Scheduled)
                .ToList();

            var downloadTasks = scheduledDownloads.Select(download => 
                ExecuteDownloadAsync(task, download, semaphore, cts.Token));

            await Task.WhenAll(downloadTasks);

            // Check if all downloads are completed
            var allCompleted = task.Downloads.All(d => 
                d.Status == ScheduledDownloadStatus.Completed || 
                d.Status == ScheduledDownloadStatus.Removed ||
                d.Status == ScheduledDownloadStatus.Cancelled);

            if (allCompleted)
            {
                task.Status = ScheduledTaskStatus.Completed;
                OnTaskCompleted(new TaskEventArgs(task));
            }
            else
            {
                // Some downloads failed, mark task as failed
                var anyFailed = task.Downloads.Any(d => d.Status == ScheduledDownloadStatus.Failed);
                if (anyFailed)
                {
                    task.Status = ScheduledTaskStatus.Failed;
                    task.ErrorMessage = "One or more downloads failed";
                    OnTaskFailed(new TaskErrorEventArgs(task, new Exception("One or more downloads failed")));
                }
            }
        }
        catch (Exception ex)
        {
            task.Status = ScheduledTaskStatus.Failed;
            task.ErrorMessage = ex.Message;
            OnTaskFailed(new TaskErrorEventArgs(task, ex));
            _logger.LogError(ex, "Task {TaskId} execution failed", task.Id);
        }
        finally
        {
            task.IsRunning = false;
        }
    }

    private async Task ExecuteDownloadAsync(ScheduledTask task, ScheduledDownload download, SemaphoreSlim semaphore, CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
        
        try
        {
            download.Status = ScheduledDownloadStatus.Running;
            OnDownloadStarted(new DownloadEventArgs(task, download));

            var downloadId = await _downloadManager.StartDownloadAsync(download.Options, cancellationToken);
            download.DownloadId = downloadId;

            _logger.LogInformation("Started download {DownloadId} for task {TaskId}", downloadId, task.Id);

            // Wait for completion or cancellation
            await WaitForDownloadCompletionAsync(download, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            download.Status = ScheduledDownloadStatus.Cancelled;
        }
        catch (Exception ex)
        {
            download.Status = ScheduledDownloadStatus.Failed;
            download.ErrorMessage = ex.Message;
            OnDownloadFailed(new DownloadErrorEventArgs(task, download, ex));
            _logger.LogError(ex, "Download {DownloadId} failed in task {TaskId}", download.Id, task.Id);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task WaitForDownloadCompletionAsync(ScheduledDownload download, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(download.DownloadId))
            return;

        while (!cancellationToken.IsCancellationRequested)
        {
            var progress = _downloadManager.GetDownloadProgress(download.DownloadId);
            if (progress == null)
                break;

            if (progress.Status == DownloadStatus.Completed)
            {
                download.Status = ScheduledDownloadStatus.Completed;
                download.CompletedAt = DateTime.Now;
                break;
            }

            if (progress.Status == DownloadStatus.Failed || progress.Status == DownloadStatus.Cancelled)
            {
                download.Status = progress.Status == DownloadStatus.Failed ? 
                    ScheduledDownloadStatus.Failed : 
                    ScheduledDownloadStatus.Cancelled;
                download.ErrorMessage = progress.ErrorMessage;
                break;
            }

            await Task.Delay(1000, cancellationToken);
        }
    }

    private void OnDownloadManagerDownloadCompleted(object? sender, DownloadProgress progress)
    {
        var task = _tasks.Values.FirstOrDefault(t => 
            t.Downloads.Any(d => d.DownloadId == progress.DownloadId));

        if (task != null)
        {
            var download = task.Downloads.FirstOrDefault(d => d.DownloadId == progress.DownloadId);
            if (download != null)
            {
                download.Status = ScheduledDownloadStatus.Completed;
                download.CompletedAt = DateTime.Now;

                OnDownloadCompleted(new DownloadEventArgs(task, download));

                if (_options.AutoRemoveCompletedDownloads)
                {
                    task.Downloads.Remove(download);
                }
            }
        }
    }

    private void OnDownloadManagerDownloadFailed(object? sender, DownloadProgress progress)
    {
        var task = _tasks.Values.FirstOrDefault(t => 
            t.Downloads.Any(d => d.DownloadId == progress.DownloadId));

        if (task != null)
        {
            var download = task.Downloads.FirstOrDefault(d => d.DownloadId == progress.DownloadId);
            if (download != null)
            {
                download.Status = ScheduledDownloadStatus.Failed;
                download.ErrorMessage = progress.ErrorMessage;

                OnDownloadFailed(new DownloadErrorEventArgs(task, download, new Exception(progress.ErrorMessage ?? "Download failed")));
            }
        }
    }

    // Event invocation methods
    protected virtual void OnTaskStarted(TaskEventArgs e)
    {
        lock (_eventLock)
        {
            TaskStarted?.Invoke(this, e);
        }
    }

    protected virtual void OnTaskCompleted(TaskEventArgs e)
    {
        lock (_eventLock)
        {
            TaskCompleted?.Invoke(this, e);
        }
    }

    protected virtual void OnTaskStopped(TaskEventArgs e)
    {
        lock (_eventLock)
        {
            TaskStopped?.Invoke(this, e);
        }
    }

    protected virtual void OnTaskFailed(TaskErrorEventArgs e)
    {
        lock (_eventLock)
        {
            TaskFailed?.Invoke(this, e);
        }
    }

    protected virtual void OnDownloadStarted(DownloadEventArgs e)
    {
        lock (_eventLock)
        {
            DownloadStarted?.Invoke(this, e);
        }
    }

    protected virtual void OnDownloadCompleted(DownloadEventArgs e)
    {
        lock (_eventLock)
        {
            DownloadCompleted?.Invoke(this, e);
        }
    }

    protected virtual void OnDownloadFailed(DownloadErrorEventArgs e)
    {
        lock (_eventLock)
        {
            DownloadFailed?.Invoke(this, e);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _schedulerTimer?.Dispose();

        // Stop all running tasks
        var runningTasks = _tasks.Values.Where(t => t.IsRunning).ToList();
        foreach (var task in runningTasks)
        {
            _ = StopTaskAsync(task.Id);
        }

        // Dispose all semaphores and cancellation tokens
        foreach (var semaphore in _taskSemaphores.Values)
        {
            semaphore.Dispose();
        }

        foreach (var cts in _taskCancellationTokens.Values)
        {
            cts.Dispose();
        }

        _taskSemaphores.Clear();
        _taskCancellationTokens.Clear();

        // Unsubscribe from download manager events
        _downloadManager.DownloadCompleted -= OnDownloadManagerDownloadCompleted;
        _downloadManager.DownloadFailed -= OnDownloadManagerDownloadFailed;

        _disposed = true;

        _logger.LogInformation("Download Task Scheduler disposed");

        GC.SuppressFinalize(this);
    }
}