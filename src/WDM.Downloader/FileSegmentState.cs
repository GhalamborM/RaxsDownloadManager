namespace WDM.Downloader
{
    public enum FileSegmentState
    {
        None,
        Connecting,
        Downloading,
        Paused,
        Failed,
        Error
    }
}
