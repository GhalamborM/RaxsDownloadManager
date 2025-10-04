using Microsoft.Extensions.Logging;
using RD.Core.Interfaces;
using RD.Core.Models;
using System.Net;

namespace RD.Core.Services;

internal class DownloadInfoProvider(IHttpClientFactory httpClientFactory, ILogger<DownloadInfoProvider> logger) : IDownloadInfoProvider
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<DownloadInfoProvider> _logger = logger;

    public async Task<DownloadInfo> GetDownloadInfoAsync(string url, DownloadOptions options, 
        CancellationToken cancellationToken = default)
    {
        using var client = _httpClientFactory.CreateClient(options);
        
        try
        {
            _logger.LogDebug("Getting download info for URL: {Url}", url);

            using var request = new HttpRequestMessage(HttpMethod.Head, url);
            using var response = await client.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.MethodNotAllowed)
            {
                // Some servers don't support HEAD, try GET with range
                return await GetInfoWithRangeRequest(client, url, cancellationToken);
            }

            response.EnsureSuccessStatusCode();

            DownloadInfo info = new()
            {
                ContentLength = response.Content.Headers.ContentLength ?? -1,
                SupportsPartialContent = response.Headers.AcceptRanges?.Contains("bytes") == true ||
                                       response.StatusCode == HttpStatusCode.PartialContent,
                ContentType = response.Content.Headers.ContentType?.MediaType,
                ETag = response.Headers.ETag?.Tag,
                LastModified = response.Content.Headers.LastModified?.DateTime,
                // Try to extract filename from Content-Disposition or URL
                FileName = GetFileNameFromResponse(response, url)
            };

            _logger.LogDebug("Download info retrieved - Length: {Length}, Supports partial: {SupportsPartial}", 
                info.ContentLength, info.SupportsPartialContent);

            return info;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get download info for URL: {Url}", url);
            throw;
        }
    }

    private static async Task<DownloadInfo> GetInfoWithRangeRequest(HttpClient client, string url, 
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(0, 0);

        using var response = await client.SendAsync(request, cancellationToken);
        
        var info = new DownloadInfo
        {
            SupportsPartialContent = response.StatusCode == HttpStatusCode.PartialContent,
            ContentType = response.Content.Headers.ContentType?.MediaType,
            ETag = response.Headers.ETag?.Tag,
            LastModified = response.Content.Headers.LastModified?.DateTime,
            FileName = GetFileNameFromResponse(response, url)
        };

        if (response.StatusCode == HttpStatusCode.PartialContent && 
            response.Content.Headers.ContentRange != null)
        {
            info.ContentLength = response.Content.Headers.ContentRange.Length ?? -1;
        }
        else
        {
            info.ContentLength = response.Content.Headers.ContentLength ?? -1;
        }

        return info;
    }

    private static string? GetFileNameFromResponse(HttpResponseMessage response, string url)
    {
        // Try Content-Disposition header first
        var contentDisposition = response.Content.Headers.ContentDisposition;
        if (!string.IsNullOrEmpty(contentDisposition?.FileName))
        {
            return contentDisposition.FileName.Trim('"');
        }

        // Extract from URL
        try
        {
            var uri = new Uri(url);
            var fileName = Path.GetFileName(uri.LocalPath);
            if (!string.IsNullOrEmpty(fileName) && fileName != "/")
            {
                return fileName;
            }
        }
        catch
        {
            // Ignore URL parsing errors
        }

        return null;
    }
}