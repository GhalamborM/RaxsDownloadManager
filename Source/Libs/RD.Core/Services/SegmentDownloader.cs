using Microsoft.Extensions.Logging;
using RD.Core.Interfaces;
using RD.Core.Models;
using System.Net;

namespace RD.Core.Services;

internal class SegmentDownloader(IHttpClientFactory httpClientFactory, ILogger<SegmentDownloader> logger) : ISegmentDownloader
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<SegmentDownloader> _logger = logger;

    public async Task DownloadSegmentAsync(DownloadSegment segment, DownloadOptions options, 
        IProgress<long>? progress = null, CancellationToken cancellationToken = default)
    {
        var retryCount = 0;
        var maxRetries = options.RetryAttempts;

        while (retryCount <= maxRetries)
        {
            try
            {
                await DownloadSegmentInternal(segment, options, progress, cancellationToken);
                segment.Status = DownloadStatus.Completed;
                return;
            }
            catch (Exception ex) when (retryCount < maxRetries && !cancellationToken.IsCancellationRequested)
            {
                retryCount++;
                segment.ErrorMessage = ex.Message;
                
                _logger.LogWarning(ex, "Segment {SegmentId} download failed, retry {RetryCount}/{MaxRetries}", 
                    segment.Id, retryCount, maxRetries);

                if (retryCount <= maxRetries)
                {
                    await Task.Delay(options.RetryDelay, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                segment.Status = DownloadStatus.Failed;
                segment.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Segment {SegmentId} download failed permanently", segment.Id);
                throw;
            }
        }
    }

    private async Task DownloadSegmentInternal(DownloadSegment segment, DownloadOptions options, 
        IProgress<long>? progress, CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient(options);
        using var request = new HttpRequestMessage(HttpMethod.Get, options.Url);

        // Resume from where we left off if the file exists
        var startByte = segment.StartByte + segment.DownloadedBytes;
        if (startByte <= segment.EndByte)
        {
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(startByte, segment.EndByte);
        }
        else
        {
            // Segment already completed
            return;
        }

        _logger.LogDebug("Downloading segment {SegmentId} bytes {StartByte}-{EndByte}", 
            segment.Id, startByte, segment.EndByte);

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        
        if (response.StatusCode != HttpStatusCode.PartialContent && response.StatusCode != HttpStatusCode.OK)
        {
            throw new HttpRequestException($"Unexpected status code: {response.StatusCode}");
        }

        using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var fileStream = new FileStream(segment.TempFilePath, FileMode.OpenOrCreate, FileAccess.Write);
        
        fileStream.Seek(segment.DownloadedBytes, SeekOrigin.Begin);

        var buffer = new byte[options.BufferSize];
        int bytesRead;
        var totalBytesRead = segment.DownloadedBytes;

        while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            totalBytesRead += bytesRead;
            segment.DownloadedBytes = totalBytesRead;

            progress?.Report(bytesRead);

            if (totalBytesRead >= segment.Length)
            {
                break;
            }
        }

        await fileStream.FlushAsync(cancellationToken);
        
        _logger.LogDebug("Segment {SegmentId} completed, downloaded {Bytes} bytes", 
            segment.Id, segment.DownloadedBytes);
    }
}