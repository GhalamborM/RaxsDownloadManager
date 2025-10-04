namespace RD.Core.Models;
public class DownloadSummary
{
    public int TotalDownloads { get; set; }
    public int CompletedDownloads { get; set; }
    public int ActiveDownloads { get; set; }
    public int PausedDownloads { get; set; }
    public int FailedDownloads { get; set; }
    public long TotalBytes { get; set; }
    public long TotalDownloadedBytes { get; set; }
    public double AverageSpeed { get; set; }
    public double OverallPercentage => TotalBytes > 0 ? (double)TotalDownloadedBytes / TotalBytes * 100 : 0;
}
