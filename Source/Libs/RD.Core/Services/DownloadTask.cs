using RD.Core.Models;
using System.Diagnostics;

namespace RD.Core.Services;

public partial class DownloadManager
{
    private class DownloadTask
    {
        public string Id { get; set; } = string.Empty;
        public DownloadOptions Options { get; set; } = new();
        public DownloadProgress Progress { get; set; } = new();
        public CancellationTokenSource CancellationTokenSource { get; set; } = new();
        public CancellationTokenSource? PauseCancellationTokenSource { get; set; }
        public Stopwatch Stopwatch { get; set; } = new();
        public List<DownloadSegment>? Segments { get; set; }
        public bool IsPaused { get; set; }
        public long TotalBytesDownloaded;
        public readonly object StateLock = new object();
        public Task? ExecutionTask { get; set; }
    }
}
