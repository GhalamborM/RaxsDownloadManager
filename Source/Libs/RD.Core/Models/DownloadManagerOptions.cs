namespace RD.Core.Models;

public class DownloadManagerOptions
{
    public int MaxConcurrentDownloads { get; set; } = 3;
    public string TempDirectory { get; set; } = Path.GetTempPath();
    public string DefaultDownloadDirectory { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
    public int DefaultMaxSegments { get; set; } = 8;
    public int DefaultBufferSize { get; set; } = 8192;
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public int DefaultRetryAttempts { get; set; } = 3;
    public TimeSpan DefaultRetryDelay { get; set; } = TimeSpan.FromSeconds(2);
    public TimeSpan ProgressReportInterval { get; set; } = TimeSpan.FromMilliseconds(500);
    public bool CleanupTempFilesOnSuccess { get; set; } = true;
    public bool CleanupTempFilesOnFailure { get; set; } = false;
    
    public bool UseCategorization { get; set; } = true;
    public List<FileCategory> FileCategories { get; set; } = FileCategoryDefaults.GetDefaultCategories();
    
    public bool RunAtStartup { get; set; } = true;
    public bool MonitorClipboardForUrls { get; set; } = true;
}