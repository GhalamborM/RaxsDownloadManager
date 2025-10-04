namespace RD.Core.Models;

public class DownloadSegment
{
    public int Id { get; set; }
    public string DownloadId { get; set; } = string.Empty;
    public long StartByte { get; set; }
    public long EndByte { get; set; }
    public long DownloadedBytes { get; set; }
    public string TempFilePath { get; set; } = string.Empty;
    public DownloadStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public long Length => EndByte - StartByte + 1;
    public bool IsCompleted => DownloadedBytes >= Length;
}