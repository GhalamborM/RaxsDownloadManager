using Microsoft.Extensions.Logging;
using RD.Core.Interfaces;

namespace RD.Core.Services;

internal class FileMerger(ILogger<FileMerger> logger) : IFileMerger
{
    private readonly ILogger<FileMerger> _logger = logger;

    public async Task MergeSegmentsAsync(IEnumerable<string> segmentFilePaths, string outputPath, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting to merge {Count} segments into {OutputPath}", 
            segmentFilePaths.Count(), outputPath);

        var tempOutputPath = outputPath + ".merging";

        try
        {
            using var outputStream = new FileStream(tempOutputPath, FileMode.Create, FileAccess.Write);

            foreach (var segmentPath in segmentFilePaths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!File.Exists(segmentPath))
                {
                    throw new FileNotFoundException($"Segment file not found: {segmentPath}");
                }

                using var segmentStream = new FileStream(segmentPath, FileMode.Open, FileAccess.Read);
                await segmentStream.CopyToAsync(outputStream, cancellationToken);

                _logger.LogDebug("Merged segment: {SegmentPath}", segmentPath);
            }

            await outputStream.FlushAsync(cancellationToken);
        }
        catch (Exception ex){
            _logger.LogError(ex, "Failed to merge segments into {OutputPath}", outputPath);
            if (File.Exists(tempOutputPath))
            {
                File.Delete(tempOutputPath);
            }
            throw;
        }

        // Atomically move the temp file to the final location
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }
        
        File.Move(tempOutputPath, outputPath);
        
        _logger.LogInformation("Successfully merged segments into {OutputPath}", outputPath);
    }
}