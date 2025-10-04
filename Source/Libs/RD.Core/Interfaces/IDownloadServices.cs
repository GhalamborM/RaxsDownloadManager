using RD.Core.Models;

namespace RD.Core.Interfaces;

public interface IHttpClientFactory
{
    HttpClient CreateClient(DownloadOptions options);
}

public interface IDownloadInfoProvider
{
    Task<DownloadInfo> GetDownloadInfoAsync(string url, DownloadOptions options, 
        CancellationToken cancellationToken = default);
}

public class DownloadInfo
{
    public long ContentLength { get; set; }
    public bool SupportsPartialContent { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public string? ETag { get; set; }
    public DateTime? LastModified { get; set; }
}