namespace Scheduler.Models;

public enum ScheduledTaskStatus
{
    Scheduled,
    Running,
    Completed,
    Cancelled,
    Failed,
    Stopped
}