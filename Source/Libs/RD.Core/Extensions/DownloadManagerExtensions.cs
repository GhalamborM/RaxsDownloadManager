using RD.Core.Interfaces;
using RD.Core.Models;

namespace RD.Core.Extensions;

/// <summary>
/// Extension methods to simplify common download operations
/// </summary>
public static class DownloadManagerExtensions
{
    /// <summary>
    /// Downloads a file with basic options
    /// </summary>
    public static Task<string> DownloadFileAsync(this IDownloadManager downloadManager, 
        string url, string filePath, CancellationToken cancellationToken = default)
    {
        var options = new DownloadOptions
        {
            Url = url,
            FilePath = filePath
        };

        return downloadManager.StartDownloadAsync(options, cancellationToken);
    }

    /// <summary>
    /// Downloads a file with custom segment count
    /// </summary>
    public static Task<string> DownloadFileAsync(this IDownloadManager downloadManager, 
        string url, string filePath, int maxSegments, CancellationToken cancellationToken = default)
    {
        var options = new DownloadOptions
        {
            Url = url,
            FilePath = filePath,
            MaxSegments = maxSegments
        };

        return downloadManager.StartDownloadAsync(options, cancellationToken);
    }

    /// <summary>
    /// Downloads a file with basic authentication
    /// </summary>
    public static Task<string> DownloadFileWithBasicAuthAsync(this IDownloadManager downloadManager,
        string url, string filePath, string username, string password, 
        CancellationToken cancellationToken = default)
    {
        var options = new DownloadOptions
        {
            Url = url,
            FilePath = filePath,
            Authentication = new AuthenticationConfiguration
            {
                Type = AuthenticationType.Basic,
                Username = username,
                Password = password
            }
        };

        return downloadManager.StartDownloadAsync(options, cancellationToken);
    }

    /// <summary>
    /// Downloads a file with bearer token authentication
    /// </summary>
    public static Task<string> DownloadFileWithBearerTokenAsync(this IDownloadManager downloadManager,
        string url, string filePath, string token, CancellationToken cancellationToken = default)
    {
        var options = new DownloadOptions
        {
            Url = url,
            FilePath = filePath,
            Authentication = new AuthenticationConfiguration
            {
                Type = AuthenticationType.Bearer,
                Token = token
            }
        };

        return downloadManager.StartDownloadAsync(options, cancellationToken);
    }

    /// <summary>
    /// Downloads a file through a proxy
    /// </summary>
    public static Task<string> DownloadFileWithProxyAsync(this IDownloadManager downloadManager,
        string url, string filePath, string proxyHost, int proxyPort,
        string? proxyUsername = null, string? proxyPassword = null,
        CancellationToken cancellationToken = default)
    {
        var options = new DownloadOptions
        {
            Url = url,
            FilePath = filePath,
            Proxy = new ProxyConfiguration
            {
                Host = proxyHost,
                Port = proxyPort,
                Username = proxyUsername,
                Password = proxyPassword
            }
        };

        return downloadManager.StartDownloadAsync(options, cancellationToken);
    }

    /// <summary>
    /// Downloads a file and waits for completion, returning the final progress
    /// </summary>
    public static async Task<DownloadProgress> DownloadAndWaitAsync(this IDownloadManager downloadManager,
        DownloadOptions options, CancellationToken cancellationToken = default)
    {
        var downloadId = await downloadManager.StartDownloadAsync(options, cancellationToken);
        
        // Wait for completion
        while (!cancellationToken.IsCancellationRequested)
        {
            var progress = downloadManager.GetDownloadProgress(downloadId) ?? throw new InvalidOperationException("Download not found");
            if (progress.Status == DownloadStatus.Completed ||
                progress.Status == DownloadStatus.Failed ||
                progress.Status == DownloadStatus.Cancelled)
            {
                return progress;
            }

            await Task.Delay(100, cancellationToken);
        }

        throw new OperationCanceledException();
    }

    /// <summary>
    /// Downloads multiple files concurrently
    /// </summary>
    public static async Task<Dictionary<string, string>> DownloadMultipleFilesAsync(
        this IDownloadManager downloadManager, 
        Dictionary<string, string> urlToFilePathMap,
        CancellationToken cancellationToken = default)
    {
        var downloadTasks = urlToFilePathMap.Select(async kvp =>
        {
            var downloadId = await downloadManager.DownloadFileAsync(kvp.Key, kvp.Value, cancellationToken);
            return new { Url = kvp.Key, DownloadId = downloadId };
        }).ToArray();

        var results = await Task.WhenAll(downloadTasks);
        return results.ToDictionary(r => r.Url, r => r.DownloadId);
    }

    /// <summary>
    /// Gets a summary of all download progress
    /// </summary>
    public static DownloadSummary GetDownloadSummary(this IDownloadManager downloadManager)
    {
        var allProgress = downloadManager.GetAllDownloadProgress().ToList();
        
        return new DownloadSummary
        {
            TotalDownloads = allProgress.Count,
            CompletedDownloads = allProgress.Count(p => p.Status == DownloadStatus.Completed),
            ActiveDownloads = allProgress.Count(p => p.Status == DownloadStatus.Downloading),
            PausedDownloads = allProgress.Count(p => p.Status == DownloadStatus.Paused),
            FailedDownloads = allProgress.Count(p => p.Status == DownloadStatus.Failed),
            TotalBytes = allProgress.Sum(p => p.TotalBytes),
            TotalDownloadedBytes = allProgress.Sum(p => p.DownloadedBytes),
            AverageSpeed = allProgress.Where(p => p.BytesPerSecond > 0).DefaultIfEmpty().Average(p => p != null ? p.BytesPerSecond : 0)
        };
    }
}