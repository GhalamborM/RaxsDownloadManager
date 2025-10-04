using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RD.Core.Models;
using RD.Services;
using MessageBox = System.Windows.MessageBox;

namespace RD.ViewModels;

public partial class OptionsViewModel : ObservableObject
{
    private readonly IDataPersistenceService _dataPersistenceService;
    private DownloadManagerOptions _originalOptions;

    [ObservableProperty]
    private int _maxConcurrentDownloads;

    [ObservableProperty]
    private string _tempDirectory = string.Empty;

    [ObservableProperty]
    private int _defaultMaxSegments;

    [ObservableProperty]
    private int _defaultBufferSize;

    [ObservableProperty]
    private int _defaultTimeoutMinutes;

    [ObservableProperty]
    private int _defaultRetryAttempts;

    [ObservableProperty]
    private int _defaultRetryDelaySeconds;

    [ObservableProperty]
    private int _progressReportIntervalMs;

    [ObservableProperty]
    private bool _cleanupTempFilesOnSuccess;

    [ObservableProperty]
    private bool _cleanupTempFilesOnFailure;

    public OptionsViewModel(IDataPersistenceService dataPersistenceService)
    {
        _dataPersistenceService = dataPersistenceService;
        _originalOptions = new DownloadManagerOptions();
        _ = LoadOptionsAsync();
    }

    [RelayCommand]
    private void BrowseTempDirectory()
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select Temporary Directory",
            SelectedPath = TempDirectory,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            TempDirectory = dialog.SelectedPath;
        }
    }

    [RelayCommand]
    private async Task SaveOptionsAsync()
    {
        try
        {
            var options = new DownloadManagerOptions
            {
                MaxConcurrentDownloads = MaxConcurrentDownloads,
                TempDirectory = TempDirectory,
                DefaultMaxSegments = DefaultMaxSegments,
                DefaultBufferSize = DefaultBufferSize,
                DefaultTimeout = TimeSpan.FromMinutes(DefaultTimeoutMinutes),
                DefaultRetryAttempts = DefaultRetryAttempts,
                DefaultRetryDelay = TimeSpan.FromSeconds(DefaultRetryDelaySeconds),
                ProgressReportInterval = TimeSpan.FromMilliseconds(ProgressReportIntervalMs),
                CleanupTempFilesOnSuccess = CleanupTempFilesOnSuccess,
                CleanupTempFilesOnFailure = CleanupTempFilesOnFailure
            };

            await _dataPersistenceService.SaveOptionsAsync(options);
            _originalOptions = options;

            MessageBox.Show("Options saved successfully. Please restart the application for changes to take effect.", 
                "Options Saved", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save options: {ex.Message}", "Error", 
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ResetToDefaults()
    {
        var defaults = new DownloadManagerOptions();
        LoadFromOptions(defaults);
    }

    [RelayCommand]
    private void Cancel()
    {
        LoadFromOptions(_originalOptions);
    }

    private async Task LoadOptionsAsync()
    {
        try
        {
            _originalOptions = await _dataPersistenceService.LoadOptionsAsync();
            LoadFromOptions(_originalOptions);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load options: {ex.Message}", "Error", 
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            LoadFromOptions(new DownloadManagerOptions());
        }
    }

    private void LoadFromOptions(DownloadManagerOptions options)
    {
        MaxConcurrentDownloads = options.MaxConcurrentDownloads;
        TempDirectory = options.TempDirectory;
        DefaultMaxSegments = options.DefaultMaxSegments;
        DefaultBufferSize = options.DefaultBufferSize;
        DefaultTimeoutMinutes = (int)options.DefaultTimeout.TotalMinutes;
        DefaultRetryAttempts = options.DefaultRetryAttempts;
        DefaultRetryDelaySeconds = (int)options.DefaultRetryDelay.TotalSeconds;
        ProgressReportIntervalMs = (int)options.ProgressReportInterval.TotalMilliseconds;
        CleanupTempFilesOnSuccess = options.CleanupTempFilesOnSuccess;
        CleanupTempFilesOnFailure = options.CleanupTempFilesOnFailure;
    }
}