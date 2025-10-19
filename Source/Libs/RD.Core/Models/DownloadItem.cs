using RD.Core.Helpers;
using RD.Core.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RD.Core.Models;

public class DownloadItem : INotifyPropertyChanged
{
    private string _downloadId = string.Empty;
    private string _fileName = string.Empty;
    private string _url = string.Empty;
    private string _filePath = string.Empty;
    private string? _referrerUrl;
    private long _totalBytes;
    private long _downloadedBytes;
    private DownloadStatus _status;
    private double _percentageComplete;
    private long _bytesPerSecond;
    private TimeSpan _elapsedTime;
    private TimeSpan? _estimatedTimeRemaining;
    private int _activeSegments;
    private string? _errorMessage;
    private DateTime _lastUpdated;
    private DateTime _createdAt;
    
    // Store download options for restoration
    private int _maxSegments = 4;
    private bool _enableResume = true;
    private bool _overwriteExisting = false;
    private TimeSpan _retryDelay = TimeSpan.FromSeconds(2);
    private int _retryAttempts = 3;
    private int _bufferSize = 8192;
    private TimeSpan _timeout = TimeSpan.FromMinutes(5);

    public string DownloadId
    {
        get => _downloadId;
        set => SetProperty(ref _downloadId, value);
    }

    public string FileName
    {
        get => _fileName;
        set => SetProperty(ref _fileName, value);
    }

    public string Url
    {
        get => _url;
        set => SetProperty(ref _url, value);
    }

    public string FilePath
    {
        get => _filePath;
        set => SetProperty(ref _filePath, value);
    }

    public string? ReferrerUrl
    {
        get => _referrerUrl;
        set => SetProperty(ref _referrerUrl, value);
    }

    public long TotalBytes
    {
        get => _totalBytes;
        set => SetProperty(ref _totalBytes, value);
    }

    public long DownloadedBytes
    {
        get => _downloadedBytes;
        set
        {
            SetProperty(ref _downloadedBytes, value);
            OnPropertyChanged(nameof(PercentageComplete));
        }
    }

    public DownloadStatus Status
    {
        get => _status;
        set
        {
            SetProperty(ref _status, value);
            if (value is DownloadStatus.Completed)
            {
                PercentageComplete = 100;
                BytesPerSecond = -1;
                EstimatedTimeRemaining = null;
            }
        }
    }

    public double PercentageComplete
    {
        get => _percentageComplete;
        set => SetProperty(ref _percentageComplete, value);
    }

    public long BytesPerSecond
    {
        get => _bytesPerSecond;
        set => SetProperty(ref _bytesPerSecond, value);
    }

    public TimeSpan ElapsedTime
    {
        get => _elapsedTime;
        set => SetProperty(ref _elapsedTime, value);
    }

    public TimeSpan? EstimatedTimeRemaining
    {
        get => _estimatedTimeRemaining;
        set => SetProperty(ref _estimatedTimeRemaining, value);
    }

    public int ActiveSegments
    {
        get => _activeSegments;
        set => SetProperty(ref _activeSegments, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public DateTime LastUpdated
    {
        get => _lastUpdated;
        set => SetProperty(ref _lastUpdated, value);
    }

    public DateTime CreatedAt
    {
        get => _createdAt;
        set => SetProperty(ref _createdAt, value);
    }

    // Download options properties for restoration
    public int MaxSegments
    {
        get => _maxSegments;
        set => SetProperty(ref _maxSegments, value);
    }

    public bool EnableResume
    {
        get => _enableResume;
        set => SetProperty(ref _enableResume, value);
    }

    public bool OverwriteExisting
    {
        get => _overwriteExisting;
        set => SetProperty(ref _overwriteExisting, value);
    }

    public TimeSpan RetryDelay
    {
        get => _retryDelay;
        set => SetProperty(ref _retryDelay, value);
    }

    public int RetryAttempts
    {
        get => _retryAttempts;
        set => SetProperty(ref _retryAttempts, value);
    }

    public int BufferSize
    {
        get => _bufferSize;
        set => SetProperty(ref _bufferSize, value);
    }

    public TimeSpan Timeout
    {
        get => _timeout;
        set => SetProperty(ref _timeout, value);
    }

    public string StatusText => Helper.GetStatusText(Status);

    public string SizeText => IsCompleted ? Helper.FormatBytes(TotalBytes) : $"{Helper.FormatBytes(DownloadedBytes)} / {Helper.FormatBytes(TotalBytes)}";

    public string SpeedText => IsCompleted ? "" : $"{Helper.FormatBytes(BytesPerSecond)}/s";

    private bool IsCompleted => Status is DownloadStatus.Completed;
    public void UpdateFromProgress(DownloadProgress progress)
    {
        DownloadId = progress.DownloadId;
        FileName = progress.FileName;
        TotalBytes = progress.TotalBytes;
        DownloadedBytes = progress.DownloadedBytes;
        Status = progress.Status;
        PercentageComplete = progress.PercentageComplete;
        BytesPerSecond = progress.BytesPerSecond;
        ElapsedTime = progress.ElapsedTime;
        EstimatedTimeRemaining = progress.EstimatedTimeRemaining;
        ActiveSegments = progress.ActiveSegments;
        ErrorMessage = progress.ErrorMessage;
        LastUpdated = progress.LastUpdated;

        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(SizeText));
        OnPropertyChanged(nameof(SpeedText));
    }

    public DownloadOptions ToDownloadOptions()
    {
        return new DownloadOptions
        {
            Url = Url,
            FilePath = FilePath,
            MaxSegments = MaxSegments,
            EnableResume = EnableResume,
            OverwriteExisting = OverwriteExisting,
            RetryDelay = RetryDelay,
            RetryAttempts = RetryAttempts,
            BufferSize = BufferSize,
            Timeout = Timeout
        };
    }

    public DownloadProgress ToDownloadProgress()
    {
        return new DownloadProgress
        {
            DownloadId = DownloadId,
            FileName = FileName,
            TotalBytes = TotalBytes,
            DownloadedBytes = DownloadedBytes,
            BytesPerSecond = BytesPerSecond,
            ElapsedTime = ElapsedTime,
            EstimatedTimeRemaining = EstimatedTimeRemaining,
            Status = Status,
            ActiveSegments = ActiveSegments,
            ErrorMessage = ErrorMessage,
            LastUpdated = LastUpdated
        };
    }

    public void UpdateFromOptions(DownloadOptions options)
    {
        MaxSegments = options.MaxSegments;
        EnableResume = options.EnableResume;
        OverwriteExisting = options.OverwriteExisting;
        RetryDelay = options.RetryDelay;
        RetryAttempts = options.RetryAttempts;
        BufferSize = options.BufferSize;
        Timeout = options.Timeout;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}