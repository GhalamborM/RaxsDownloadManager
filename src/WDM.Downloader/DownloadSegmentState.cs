namespace WDM.Downloader
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
