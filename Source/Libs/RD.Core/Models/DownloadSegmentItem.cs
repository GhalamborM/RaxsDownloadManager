using CommunityToolkit.Mvvm.ComponentModel;
using RD.Core.Helpers;

namespace RD.Core.Models;

public partial class DownloadSegmentItem : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _downloadId = string.Empty;

    [ObservableProperty]
    private long _startByte;

    [ObservableProperty]
    private long _endByte;

    [ObservableProperty]
    private long _downloadedBytes;

    [ObservableProperty]
    private DownloadStatus _status;

    [ObservableProperty]
    private string? _errorMessage;

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
        
        //OnPropertyChanged(nameof(Length));
        //OnPropertyChanged(nameof(IsCompleted));
        //OnPropertyChanged(nameof(PercentageComplete));
        //OnPropertyChanged(nameof(StatusText));
        //OnPropertyChanged(nameof(RangeText));
        //OnPropertyChanged(nameof(ProgressText));
    }
}