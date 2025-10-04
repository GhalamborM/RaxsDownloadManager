
using RD.Core.Models;

namespace Scheduler.Models;

public class ScheduledDownload
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Url { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DownloadOptions Options { get; set; } = new();
    public ScheduledDownloadStatus Status { get; set; } = ScheduledDownloadStatus.Scheduled;
    public string? DownloadId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }
}
