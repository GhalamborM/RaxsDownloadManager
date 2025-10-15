using RD.Models;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RD.Services;

public class UpdateService : IUpdateService
{
    private readonly HttpClient _httpClient;
    private const string GitHubApiUrl = "https://api.github.com/repos/RaxsStudio/RaxsDownloadManager/releases";
    private const string GitHubRepoUrl = "https://github.com/RaxsStudio/RaxsDownloadManager";

    public UpdateService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "RaxsDownloadManager-UpdateChecker");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
    }

    public Version GetCurrentVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version ?? new Version(1, 0, 0, 0);
    }

    public async Task<UpdateInfo> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var currentVersion = GetCurrentVersion();
            var releases = await GetAllReleasesAsync(cancellationToken);

            // Filter out drafts and prereleases
            var stableReleases = releases
                .Where(r => !r.Draft && !r.Prerelease)
                .OrderByDescending(r => r.PublishedAt)
                .ToList();

            if (stableReleases.Count == 0)
            {
                return new UpdateInfo
                {
                    IsUpdateAvailable = false,
                    CurrentVersion = currentVersion,
                    LatestVersion = currentVersion
                };
            }

            var latestRelease = stableReleases.First();
            var latestVersion = ParseVersionFromTag(latestRelease.TagName);

            var isUpdateAvailable = latestVersion > currentVersion;

            return new UpdateInfo
            {
                IsUpdateAvailable = isUpdateAvailable,
                CurrentVersion = currentVersion,
                LatestVersion = latestVersion,
                ReleaseNotes = latestRelease.Body,
                DownloadUrl = latestRelease.HtmlUrl,
                PublishedAt = latestRelease.PublishedAt,
                ReleaseName = latestRelease.Name,
                Assets = latestRelease.Assets
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to check for updates: {ex.Message}");
            return new UpdateInfo
            {
                IsUpdateAvailable = false,
                CurrentVersion = GetCurrentVersion(),
                LatestVersion = GetCurrentVersion()
            };
        }
    }

    public async Task<List<GitHubRelease>> GetAllReleasesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(GitHubApiUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var releases = JsonSerializer.Deserialize<List<GitHubRelease>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return releases ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to get releases: {ex.Message}");
            return [];
        }
    }

    public void OpenReleasePage(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open release page: {ex.Message}");
        }
    }

    private Version ParseVersionFromTag(string tagName)
    {
        try
        {
            // Remove 'v' prefix if present (e.g., "v1.0.1" -> "1.0.1")
            var versionString = tagName.TrimStart('v', 'V');

            // Extract version numbers using regex
            var match = Regex.Match(versionString, @"^(\d+)\.(\d+)\.(\d+)(?:\.(\d+))?");
            
            if (match.Success)
            {
                var major = int.Parse(match.Groups[1].Value);
                var minor = int.Parse(match.Groups[2].Value);
                var build = int.Parse(match.Groups[3].Value);
                var revision = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : 0;

                return new Version(major, minor, build, revision);
            }

            // Fallback: try to parse directly
            if (Version.TryParse(versionString, out var version))
            {
                return version;
            }

            return new Version(0, 0, 0, 0);
        }
        catch
        {
            return new Version(0, 0, 0, 0);
        }
    }
}
