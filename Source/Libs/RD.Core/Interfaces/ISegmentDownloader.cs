using RD.Core.Models;

namespace RD.Core.Interfaces;

public interface ISegmentDownloader
{
    Task DownloadSegmentAsync(DownloadSegment segment, DownloadOptions options, 
        IProgress<long>? progress = null, CancellationToken cancellationToken = default);
}