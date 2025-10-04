using RD.Core.Models;
using Scheduler.Events;
using Scheduler.Models;

namespace Scheduler.Interfaces;

public interface ITaskScheduler : IDisposable
{
    /// <summary>
    /// Global enable/disable state for all tasks
    /// </summary>
    bool IsGloballyEnabled { get; set; }

    /// <summary>
    /// Gets all scheduled tasks
    /// </summary>
    IReadOnlyList<ScheduledTask> Tasks { get; }

    /// <summary>
    /// Creates a new scheduled task
    /// </summary>
    /// <param name="name">Optional name for the task</param>
    /// <param name="startTime">When the task should start</param>
    /// <param name="endTime">Optional end time when the task should stop</param>
    /// <param name="maxConcurrentDownloads">Optional maximum concurrent downloads for this task</param>
    /// <returns>The created task</returns>
    ScheduledTask CreateTask(string? name = null, DateTime? startTime = null, DateTime? endTime = null, int? maxConcurrentDownloads = null);

    /// <summary>
    /// Adds a download to a scheduled task
    /// </summary>
    /// <param name="taskId">The task ID</param>
    /// <param name="url">Download URL</param>
    /// <param name="filePath">Target file path</param>
    /// <param name="options">Download options (optional)</param>
    /// <returns>The created scheduled download</returns>
    ScheduledDownload AddDownloadToTask(string taskId, string url, string filePath, DownloadOptions? options = null);

    /// <summary>
    /// Removes a download from a task
    /// </summary>
    /// <param name="taskId">The task ID</param>
    /// <param name="downloadId">The download ID</param>
    /// <returns>True if removed successfully</returns>
    bool RemoveDownloadFromTask(string taskId, string downloadId);

    /// <summary>
    /// Moves a download from one task to another
    /// </summary>
    /// <param name="fromTaskId">Source task ID</param>
    /// <param name="toTaskId">Destination task ID</param>
    /// <param name="downloadId">Download ID to move</param>
    /// <returns>True if moved successfully</returns>
    bool MoveDownload(string fromTaskId, string toTaskId, string downloadId);

    /// <summary>
    /// Enables or disables a specific task
    /// </summary>
    /// <param name="taskId">The task ID</param>
    /// <param name="enabled">Whether the task should be enabled</param>
    /// <returns>True if the operation succeeded</returns>
    bool SetTaskEnabled(string taskId, bool enabled);

    /// <summary>
    /// Starts a specific task immediately (ignores start time)
    /// </summary>
    /// <param name="taskId">The task ID</param>
    /// <returns>True if started successfully</returns>
    Task<bool> StartTaskAsync(string taskId);

    /// <summary>
    /// Stops a specific task
    /// </summary>
    /// <param name="taskId">The task ID</param>
    /// <returns>True if stopped successfully</returns>
    Task<bool> StopTaskAsync(string taskId);

    /// <summary>
    /// Removes a task and all its downloads
    /// </summary>
    /// <param name="taskId">The task ID</param>
    /// <returns>True if removed successfully</returns>
    Task<bool> RemoveTaskAsync(string taskId);

    /// <summary>
    /// Gets a specific task by ID
    /// </summary>
    /// <param name="taskId">The task ID</param>
    /// <returns>The task or null if not found</returns>
    ScheduledTask? GetTask(string taskId);

    /// <summary>
    /// Updates task properties
    /// </summary>
    /// <param name="taskId">The task ID</param>
    /// <param name="name">New name (optional)</param>
    /// <param name="startTime">New start time (optional)</param>
    /// <param name="endTime">New end time (optional)</param>
    /// <param name="maxConcurrentDownloads">New max concurrent downloads (optional)</param>
    /// <returns>True if updated successfully</returns>
    bool UpdateTask(string taskId, string? name = null, DateTime? startTime = null, DateTime? endTime = null, int? maxConcurrentDownloads = null);

    // Events
    event EventHandler<TaskEventArgs> TaskStarted;
    event EventHandler<TaskEventArgs> TaskCompleted;
    event EventHandler<TaskEventArgs> TaskStopped;
    event EventHandler<TaskErrorEventArgs> TaskFailed;
    event EventHandler<DownloadEventArgs> DownloadStarted;
    event EventHandler<DownloadEventArgs> DownloadCompleted;
    event EventHandler<DownloadErrorEventArgs> DownloadFailed;
}