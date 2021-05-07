namespace HttpDownloadManager
{
    public enum DownloadSegmentState
    {
        None,
        Connecting,
        Downloading,
        Paused,
        Failed,
        Error
    }
}
