using RD.Core.Models;

namespace RD.Core.Interfaces;

public interface IDownloadManager : IDisposable
{
    /// <summary>
    /// Starts a new download with the specified options
    /// </summary>
    Task<string> StartDownloadAsync(DownloadOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores a paused download from saved state
    /// </summary>
    Task<string> RestoreDownloadAsync(string downloadId, DownloadOptions options, DownloadProgress savedProgress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses an active download
    /// </summary>
    Task<bool> PauseDownloadAsync(string downloadId);

    /// <summary>
    /// Resumes a paused download
    /// </summary>
    Task<bool> ResumeDownloadAsync(string downloadId);

    /// <summary>
    /// Cancels a download
    /// </summary>
    Task<bool> CancelDownloadAsync(string downloadId);

    /// <summary>
    /// Gets the progress of a specific download
    /// </summary>
    DownloadProgress? GetDownloadProgress(string downloadId);

    /// <summary>
    /// Gets the progress of all active downloads
    /// </summary>
    IEnumerable<DownloadProgress> GetAllDownloadProgress();

    /// <summary>
    /// Gets the segments of a specific download
    /// </summary>
    IEnumerable<DownloadSegment>? GetDownloadSegments(string downloadId);

    /// <summary>
    /// Event fired when download progress is updated
    /// </summary>
    event EventHandler<DownloadProgress> ProgressChanged;

    /// <summary>
    /// Event fired when a download is completed
    /// </summary>
    event EventHandler<DownloadProgress> DownloadCompleted;

    /// <summary>
    /// Event fired when a download fails
    /// </summary>
    event EventHandler<DownloadProgress> DownloadFailed;
}