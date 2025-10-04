namespace Scheduler.Models;

public enum ScheduledDownloadStatus
{
    Scheduled,
    Running,
    Completed,
    Failed,
    Cancelled,
    Removed
}