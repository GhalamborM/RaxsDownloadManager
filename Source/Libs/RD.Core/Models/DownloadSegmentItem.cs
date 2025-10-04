using RD.Core.Helpers;

namespace RD.Core.Models;

public class DownloadSegmentItem
{
    public int Id { get; set; }
    public string DownloadId { get; set; } = string.Empty;
    public long StartByte { get; set; }
    public long EndByte { get; set; }
    public long DownloadedBytes { get; set; }
    public DownloadStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public long Length => EndByte - StartByte + 1;
    public bool IsCompleted => DownloadedBytes >= Length;
    public double PercentageComplete => Length > 0 ? (double)DownloadedBytes / Length * 100 : 0;
    public string StatusText => Helper.GetStatusText(Status);

    public string RangeText => $"{StartByte:N0} - {EndByte:N0}";
    public string ProgressText => $"{Helper.FormatBytes(DownloadedBytes)} / {Helper.FormatBytes(Length)}";


    public void UpdateFromSegment(DownloadSegment segment)
    {
        Id = segment.Id;
        DownloadId = segment.DownloadId;
        StartByte = segment.StartByte;
        EndByte = segment.EndByte;
        DownloadedBytes = segment.DownloadedBytes;
        Status = segment.Status;
        ErrorMessage = segment.ErrorMessage;
    }
}