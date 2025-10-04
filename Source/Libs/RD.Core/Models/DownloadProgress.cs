namespace RD.Core.Models;

public class DownloadProgress
{
    public string DownloadId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long TotalBytes { get; set; }
    public long DownloadedBytes { get; set; }
    public double PercentageComplete => TotalBytes > 0 ? (double)DownloadedBytes / TotalBytes * 100 : 0;
    public long BytesPerSecond { get; set; }
    public TimeSpan ElapsedTime { get; set; }
    public TimeSpan? EstimatedTimeRemaining { get; set; }
    public DownloadStatus Status { get; set; }
    public int ActiveSegments { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public enum DownloadStatus
{
    Pending,
    Downloading,
    Paused,
    Completed,
    Failed,
    Cancelled,
    Merging
}