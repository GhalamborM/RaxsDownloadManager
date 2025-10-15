namespace RD.Models;

public class UpdateInfo
{
    public bool IsUpdateAvailable { get; set; }
    public Version? CurrentVersion { get; set; }
    public Version? LatestVersion { get; set; }
    public string? ReleaseNotes { get; set; }
    public string? DownloadUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? ReleaseName { get; set; }
    public List<GitHubReleaseAsset>? Assets { get; set; }
}
