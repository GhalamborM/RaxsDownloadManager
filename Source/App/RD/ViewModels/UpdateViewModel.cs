using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RD.Controls;
using RD.Models;
using RD.Services;
using MessageBox = System.Windows.MessageBox;

namespace RD.ViewModels;

public partial class UpdateViewModel : ObservableObject
{
    private readonly IUpdateService _updateService;

    [ObservableProperty]
    private bool _isChecking;

    [ObservableProperty]
    private bool _isUpdateAvailable;

    [ObservableProperty]
    private string _currentVersion = string.Empty;

    [ObservableProperty]
    private string _latestVersion = string.Empty;

    [ObservableProperty]
    private string _releaseNotes = string.Empty;

    [ObservableProperty]
    private string _downloadUrl = string.Empty;

    [ObservableProperty]
    private DateTime? _publishedAt;

    [ObservableProperty]
    private string _releaseName = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Click 'Check for Updates' to see if a new version is available.";

    public UpdateViewModel(IUpdateService updateService)
    {
        _updateService = updateService;
        var version = _updateService.GetCurrentVersion();
        _currentVersion = $"v{version.Major}.{version.Minor}.{version.Build}";
    }

    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        try
        {
            IsChecking = true;
            StatusMessage = "Checking for updates...";

            var updateInfo = await _updateService.CheckForUpdatesAsync();

            CurrentVersion = $"v{updateInfo.CurrentVersion?.Major}.{updateInfo.CurrentVersion?.Minor}.{updateInfo.CurrentVersion?.Build}";
            
            if (updateInfo.IsUpdateAvailable && updateInfo.LatestVersion != null)
            {
                IsUpdateAvailable = true;
                LatestVersion = $"v{updateInfo.LatestVersion.Major}.{updateInfo.LatestVersion.Minor}.{updateInfo.LatestVersion.Build}";
                ReleaseNotes = updateInfo.ReleaseNotes ?? "No release notes available.";
                DownloadUrl = updateInfo.DownloadUrl ?? string.Empty;
                PublishedAt = updateInfo.PublishedAt;
                ReleaseName = updateInfo.ReleaseName ?? $"Version {LatestVersion}";
                StatusMessage = $"A new version ({LatestVersion}) is available!";
            }
            else
            {
                IsUpdateAvailable = false;
                StatusMessage = "You are using the latest version.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to check for updates: {ex.Message}";
            CustomMessageBox.ShowCustom($"Failed to check for updates: {ex.Message}", "Error", 
                MessageBoxType.Error);
        }
        finally
        {
            IsChecking = false;
        }
    }

    [RelayCommand]
    private void OpenReleasePage()
    {
        if (!string.IsNullOrEmpty(DownloadUrl))
        {
            _updateService.OpenReleasePage(DownloadUrl);
        }
    }

    [RelayCommand]
    private void OpenGitHubPage()
    {
        _updateService.OpenReleasePage("https://github.com/RaxsStudio/RaxsDownloadManager");
    }
}
