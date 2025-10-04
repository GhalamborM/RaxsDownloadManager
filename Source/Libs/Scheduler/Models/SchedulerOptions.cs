namespace Scheduler.Models;

public class SchedulerOptions
{
    /// <summary>
    /// Global enable/disable for all scheduled tasks
    /// </summary>
    public bool IsGloballyEnabled { get; set; } = true;

    /// <summary>
    /// Default maximum concurrent downloads per task (can be overridden per task)
    /// </summary>
    public int DefaultMaxConcurrentDownloads { get; set; } = 3;

    /// <summary>
    /// Interval for checking scheduled tasks
    /// </summary>
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Whether to automatically remove completed downloads from tasks
    /// </summary>
    public bool AutoRemoveCompletedDownloads { get; set; } = true;

    /// <summary>
    /// Whether to automatically remove completed/stopped tasks
    /// </summary>
    public bool AutoRemoveCompletedTasks { get; set; } = false;

    /// <summary>
    /// How long to wait before retrying a failed download
    /// </summary>
    public TimeSpan FailedDownloadRetryDelay { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Maximum number of retry attempts for failed downloads
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
}