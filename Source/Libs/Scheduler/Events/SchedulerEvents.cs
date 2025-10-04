using Scheduler.Models;

namespace Scheduler.Events;

public class TaskEventArgs(ScheduledTask task) : EventArgs
{
    public ScheduledTask Task { get; } = task;
    public DateTime Timestamp { get; } = DateTime.Now;
}

public class TaskErrorEventArgs(ScheduledTask task, Exception exception) : TaskEventArgs(task)
{
    public Exception Exception { get; } = exception;
}

public class DownloadEventArgs(ScheduledTask task, ScheduledDownload download) : EventArgs
{
    public ScheduledTask Task { get; } = task;
    public ScheduledDownload Download { get; } = download;
    public DateTime Timestamp { get; } = DateTime.Now;
}

public class DownloadErrorEventArgs(ScheduledTask task, ScheduledDownload download, Exception exception) : DownloadEventArgs(task, download)
{
    public Exception Exception { get; } = exception;
}