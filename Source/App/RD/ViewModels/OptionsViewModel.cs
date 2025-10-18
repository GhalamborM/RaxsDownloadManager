using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RD.Controls;
using RD.Core.Models;
using RD.Core.Helpers;
using RD.Localization;
using RD.Services;
using RD.Views;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows;
using DialogResult = System.Windows.Forms.DialogResult;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

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
    private string _defaultDownloadDirectory = string.Empty;

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

    [ObservableProperty]
    private bool _useCategorization;

    [ObservableProperty]
    private ObservableCollection<FileCategory> _fileCategories = new();

    [ObservableProperty]
    private FileCategory? _selectedCategory;

    public OptionsViewModel(IDataPersistenceService dataPersistenceService)
    {
        _dataPersistenceService = dataPersistenceService;
        _originalOptions = new DownloadManagerOptions();
        _ = LoadOptionsAsync();
    }

    [RelayCommand]
    private void BrowseTempDirectory()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = OptionsUtils.SelectTemporaryDirectory,
            SelectedPath = TempDirectory,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            TempDirectory = dialog.SelectedPath;
        }
    }

    [RelayCommand]
    private void BrowseDownloadDirectory()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = OptionsUtils.SelectDefaultDownloadDirectory,
            SelectedPath = DefaultDownloadDirectory,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            DefaultDownloadDirectory = dialog.SelectedPath;
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
                DefaultDownloadDirectory = DefaultDownloadDirectory,
                DefaultMaxSegments = DefaultMaxSegments,
                DefaultBufferSize = DefaultBufferSize,
                DefaultTimeout = TimeSpan.FromMinutes(DefaultTimeoutMinutes),
                DefaultRetryAttempts = DefaultRetryAttempts,
                DefaultRetryDelay = TimeSpan.FromSeconds(DefaultRetryDelaySeconds),
                ProgressReportInterval = TimeSpan.FromMilliseconds(ProgressReportIntervalMs),
                CleanupTempFilesOnSuccess = CleanupTempFilesOnSuccess,
                CleanupTempFilesOnFailure = CleanupTempFilesOnFailure,
                UseCategorization = UseCategorization,
                FileCategories = FileCategories.ToList()
            };

            // Validate categories before saving
            FileCategoryHelper.ValidateCategories(options.FileCategories);

            await _dataPersistenceService.SaveOptionsAsync(options);
            _originalOptions = options;
        }
        catch (Exception ex)
        {
            CustomMessageBox.Show($"{MessageUtils.FailedToSaveOptions} {ex.Message}", MessageUtils.Error, MessageBoxType.Error);
        }
    }

    [RelayCommand]
    private void CreateCategory()
    {
        var newCategory = new FileCategory
        {
            Name = "New Category",
            FolderName = "NewCategory",
            Extensions = new List<string>(),
            IsEnabled = true,
            IsDefault = false
        };

        var editWindow = new EditCategoryWindow
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        
        var viewModel = new EditCategoryViewModel();
        viewModel.LoadCategory(newCategory);
        editWindow.DataContext = viewModel;

        viewModel.SaveRequested += () =>
        {
            FileCategories.Add(newCategory);
            editWindow.Close();
        };

        viewModel.CancelRequested += () =>
        {
            editWindow.Close();
        };

        editWindow.ShowDialog();
    }

    [RelayCommand]
    private void EditCategory()
    {
        if (SelectedCategory == null)
            return;

        var editWindow = new EditCategoryWindow
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        
        var viewModel = new EditCategoryViewModel();
        viewModel.LoadCategory(SelectedCategory);
        editWindow.DataContext = viewModel;

        viewModel.SaveRequested += () =>
        {
            var index = FileCategories.IndexOf(SelectedCategory);
            if (index >= 0)
            {
                FileCategories[index] = SelectedCategory;
                OnPropertyChanged(nameof(FileCategories));
            }
            editWindow.Close();
        };

        viewModel.CancelRequested += () =>
        {
            editWindow.Close();
        };

        editWindow.ShowDialog();
    }

    [RelayCommand]
    private void DeleteCategory()
    {
        if (SelectedCategory == null)
            return;

        if (SelectedCategory.IsDefault)
        {
            CustomMessageBox.Show(
                "Cannot delete default categories. You can disable them instead.",
                "Cannot Delete",
                MessageBoxType.Warning);
            return;
        }

        var result = CustomMessageBox.Show(
            $"Are you sure you want to delete the category '{SelectedCategory.Name}'?",
            "Confirm Delete",
            MessageBoxType.Question,
            Controls.MessageBoxButtons.YesNo);

        if (result == MessageBoxResult.Yes)
        {
            FileCategories.Remove(SelectedCategory);
            SelectedCategory = null;
        }
    }

    [RelayCommand]
    private void ResetCategoriesToDefaults()
    {
        var result = CustomMessageBox.Show(
            "This will reset all categories to defaults and remove any custom categories. Continue?",
            "Confirm Reset",
            MessageBoxType.Question,
            Controls.MessageBoxButtons.YesNo);

        if (result == MessageBoxResult.Yes)
        {
            FileCategories.Clear();
            foreach (var category in FileCategoryDefaults.GetDefaultCategories())
            {
                FileCategories.Add(category);
            }
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
            Debug.WriteLine($"Failed to load options: {ex.Message}");
            LoadFromOptions(new DownloadManagerOptions());
        }
    }

    private void LoadFromOptions(DownloadManagerOptions options)
    {
        MaxConcurrentDownloads = options.MaxConcurrentDownloads;
        TempDirectory = options.TempDirectory;
        DefaultDownloadDirectory = options.DefaultDownloadDirectory;
        DefaultMaxSegments = options.DefaultMaxSegments;
        DefaultBufferSize = options.DefaultBufferSize;
        DefaultTimeoutMinutes = (int)options.DefaultTimeout.TotalMinutes;
        DefaultRetryAttempts = options.DefaultRetryAttempts;
        DefaultRetryDelaySeconds = (int)options.DefaultRetryDelay.TotalSeconds;
        ProgressReportIntervalMs = (int)options.ProgressReportInterval.TotalMilliseconds;
        CleanupTempFilesOnSuccess = options.CleanupTempFilesOnSuccess;
        CleanupTempFilesOnFailure = options.CleanupTempFilesOnFailure;
        UseCategorization = options.UseCategorization;

        FileCategories.Clear();
        foreach (var category in options.FileCategories)
        {
            FileCategories.Add(category);
        }
    }
}