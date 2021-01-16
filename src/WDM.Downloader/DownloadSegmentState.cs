namespace WDM.Downloaders
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
