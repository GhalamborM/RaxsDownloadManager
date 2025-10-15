using RD.Models;

namespace RD.Services;

public interface IUpdateService
{
    /// <summary>
    /// Checks if a new version is available
    /// </summary>
    Task<UpdateInfo> CheckForUpdatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current application version
    /// </summary>
    Version GetCurrentVersion();

    /// <summary>
    /// Opens the release page in the default browser
    /// </summary>
    void OpenReleasePage(string url);

    /// <summary>
    /// Gets all releases from GitHub
    /// </summary>
    Task<List<GitHubRelease>> GetAllReleasesAsync(CancellationToken cancellationToken = default);
}
