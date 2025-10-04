namespace Scheduler.Models;

public class ScheduledTask
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string? Name { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? MaxConcurrentDownloads { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsRunning { get; set; } = false;
    public ScheduledTaskStatus Status { get; set; } = ScheduledTaskStatus.Scheduled;
    public List<ScheduledDownload> Downloads { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? LastRunAt { get; set; }
    public string? ErrorMessage { get; set; }

    public bool ShouldRun => IsEnabled && 
                            Status != ScheduledTaskStatus.Completed && 
                            Status != ScheduledTaskStatus.Cancelled &&
                            DateTime.Now >= StartTime && 
                            (EndTime == null || DateTime.Now <= EndTime);

    public bool ShouldStop => EndTime.HasValue && DateTime.Now > EndTime;
}
