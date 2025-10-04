namespace RD.Core.Interfaces;

public interface IFileMerger
{
    Task MergeSegmentsAsync(IEnumerable<string> segmentFilePaths, string outputPath, 
        CancellationToken cancellationToken = default);
}