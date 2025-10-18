using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RD.Core.Interfaces;
using RD.Core.Models;
using System.Collections.ObjectModel;
using RD.Services;
using RD.Views;
using Microsoft.Extensions.DependencyInjection;
using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Application;
using Scheduler.Interfaces;

namespace RD.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IDownloadManager _downloadManager;
    private readonly IDataPersistenceService _dataPersistenceService;
    private readonly ITaskScheduler _taskScheduler;
    private readonly System.Threading.Timer _saveTimer;

    [ObservableProperty]
    private ObservableCollection<DownloadItem> _downloads = [];

    [ObservableProperty]
    private DownloadItem? _selectedDownload;

    [ObservableProperty]
    private bool _isLoading;

    public int ActiveDownloadsCount => Downloads.Count(d => d.Status == DownloadStatus.Downloading);

    public MainViewModel(IDownloadManager downloadManager, IDataPersistenceService dataPersistenceService, ITaskScheduler taskScheduler)
    {
        _downloadManager = downloadManager;
        _dataPersistenceService = dataPersistenceService;
        _taskScheduler = taskScheduler;

        // Subscribe to download manager events
        _downloadManager.ProgressChanged += OnProgressChanged;
        _downloadManager.DownloadCompleted += OnDownloadCompleted;
        _downloadManager.DownloadFailed += OnDownloadFailed;

        // Auto-save timer (save every 30 seconds)
        _saveTimer = new System.Threading.Timer(async _ => await SaveDownloadsAsync(), null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));

        _ = LoadDownloadsAsync();
    }

    [RelayCommand]
    private void ShowAddDownload()
    {
        var addDownloadWindow = App.ServiceProvider.GetRequiredService<AddDownloadWindow>();
        var viewModel = App.ServiceProvider.GetRequiredService<AddDownloadViewModel>();
        addDownloadWindow.DataContext = viewModel;
        
        viewModel.DownloadAdded += async (url, options) =>
        {
            await AddDownloadAsync(url, options);
            addDownloadWindow.Close();
        };

        addDownloadWindow.ShowDialog();
    }

    [RelayCommand]
    private void ShowOptions()
    {
        var optionsWindow = App.ServiceProvider.GetRequiredService<OptionsWindow>();
        var viewModel = App.ServiceProvider.GetRequiredService<OptionsViewModel>();
        optionsWindow.DataContext = viewModel;
        optionsWindow.ShowDialog();
    }

    [RelayCommand]
    private void ShowScheduler()
    {
        var schedulerWindow = App.ServiceProvider.GetRequiredService<SchedulerWindow>();
        var viewModel = App.ServiceProvider.GetRequiredService<SchedulerViewModel>();
        schedulerWindow.DataContext = viewModel;
        schedulerWindow.Show();
    }

    [RelayCommand]
    private void CheckForUpdates()
    {
        var updateWindow = App.ServiceProvider.GetRequiredService<UpdateWindow>();
        var viewModel = App.ServiceProvider.GetRequiredService<UpdateViewModel>();
        updateWindow.DataContext = viewModel;
        updateWindow.Owner = Application.Current.MainWindow;
        updateWindow.ShowDialog();
    }

    [RelayCommand]
    private async Task ResumeDownloadAsync()
    {
        if (SelectedDownload?.Status == DownloadStatus.Paused)
        {
            await _downloadManager.ResumeDownloadAsync(SelectedDownload.DownloadId);
        }
    }

    [RelayCommand]
    private async Task PauseDownloadAsync()
    {
        if (SelectedDownload?.Status == DownloadStatus.Downloading)
        {
            await _downloadManager.PauseDownloadAsync(SelectedDownload.DownloadId);
        }
    }

    [RelayCommand]
    private async Task RestartDownloadAsync()
    {
        if (SelectedDownload != null)
        {
            await _downloadManager.CancelDownloadAsync(SelectedDownload.DownloadId);
            
            var options = SelectedDownload.ToDownloadOptions();

            var downloadId = await _downloadManager.StartDownloadAsync(options);
            SelectedDownload.DownloadId = downloadId;
            SelectedDownload.Status = DownloadStatus.Pending;
            SelectedDownload.DownloadedBytes = 0;
            SelectedDownload.ErrorMessage = null;
        }
    }

    [RelayCommand]
    private async Task RemoveDownloadAsync()
    {
        if (SelectedDownload != null)
        {
            await _downloadManager.CancelDownloadAsync(SelectedDownload.DownloadId);
            Downloads.Remove(SelectedDownload);
            OnPropertyChanged(nameof(ActiveDownloadsCount));
            await SaveDownloadsAsync();
        }
    }

    [RelayCommand]
    private void ShowDownloadDetails()
    {
        if (SelectedDownload != null)
        {
            var detailsWindow = App.ServiceProvider.GetRequiredService<DownloadDetailsWindow>();
            var viewModel = App.ServiceProvider.GetRequiredService<DownloadDetailsViewModel>();
            viewModel.SetDownload(SelectedDownload);
            detailsWindow.DataContext = viewModel;
            detailsWindow.Show();
        }
    }

    [RelayCommand]
    private void OnDownloadItemDoubleClick()
    {
        ShowDownloadDetails();
    }

    [RelayCommand]
    private void AddUrlToTask()
    {
        if (SelectedDownload == null)
        {
            MessageBox.Show("Please select a download first.", "No Download Selected", 
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        var taskSelectionWindow = App.ServiceProvider.GetRequiredService<TaskSelectionWindow>();
        var viewModel = App.ServiceProvider.GetRequiredService<TaskSelectionViewModel>();
        taskSelectionWindow.DataContext = viewModel;
        
        viewModel.SetUrl(SelectedDownload.Url);
        
        viewModel.TaskSelected += (task, url, filePath) =>
        {
            try
            {
                _taskScheduler.AddDownloadToTask(task.Id, url, filePath);
                MessageBox.Show($"URL added to task '{task.Name}' successfully!", "Success", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                taskSelectionWindow.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add URL to task: {ex.Message}", "Error", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        };
        
        viewModel.Cancelled += () =>
        {
            taskSelectionWindow.Close();
        };

        taskSelectionWindow.Owner = Application.Current.MainWindow;
        taskSelectionWindow.ShowDialog();
    }

    public async Task AddDownloadAsync(string url, DownloadOptions options)
    {
        try
        {
            var downloadItem = new DownloadItem
            {
                Url = url,
                FilePath = options.FilePath,
                FileName = System.IO.Path.GetFileName(options.FilePath),
                Status = DownloadStatus.Pending,
                CreatedAt = DateTime.Now
            };

            // Store download options for restoration
            downloadItem.UpdateFromOptions(options);

            Downloads.Add(downloadItem);
            OnPropertyChanged(nameof(ActiveDownloadsCount));

            var downloadId = await _downloadManager.StartDownloadAsync(options);
            downloadItem.DownloadId = downloadId;

            await SaveDownloadsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to start download: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private async void OnProgressChanged(object? sender, DownloadProgress progress)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var downloadItem = Downloads.FirstOrDefault(d => d.DownloadId == progress.DownloadId);
            downloadItem?.UpdateFromProgress(progress);
            OnPropertyChanged(nameof(ActiveDownloadsCount));
        });
    }

    private async void OnDownloadCompleted(object? sender, DownloadProgress progress)
    {
        await Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            var downloadItem = Downloads.FirstOrDefault(d => d.DownloadId == progress.DownloadId);
            if (downloadItem != null)
            {
                downloadItem.UpdateFromProgress(progress);
                await SaveDownloadsAsync();
                OnPropertyChanged(nameof(ActiveDownloadsCount));
            }
        });
    }

    private async void OnDownloadFailed(object? sender, DownloadProgress progress)
    {
        await Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            var downloadItem = Downloads.FirstOrDefault(d => d.DownloadId == progress.DownloadId);
            if (downloadItem != null)
            {
                downloadItem.UpdateFromProgress(progress);
                await SaveDownloadsAsync();
                OnPropertyChanged(nameof(ActiveDownloadsCount));
            }
        });
    }

    private async Task LoadDownloadsAsync()
    {
        try
        {
            IsLoading = true;
            var savedDownloads = await _dataPersistenceService.LoadDownloadsAsync();
            
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Downloads.Clear();
                foreach (var download in savedDownloads)
                {
                    Downloads.Add(download);
                }
            });

            // Restore paused and incomplete downloads to the download manager
            foreach (var download in savedDownloads.Where(d => 
                (d.Status == DownloadStatus.Downloading || d.Status == DownloadStatus.Paused) && 
                d.Status != DownloadStatus.Completed && 
                d.Status != DownloadStatus.Failed && 
                d.Status != DownloadStatus.Cancelled))
            {
                try
                {
                    // Convert to download options and progress for restoration
                    var options = download.ToDownloadOptions();
                    var savedProgress = download.ToDownloadProgress();
                    
                    // Restore the download to the download manager as paused
                    var downloadId = await _downloadManager.RestoreDownloadAsync(download.DownloadId, options, savedProgress);
                    
                    // Update the download item status to paused and trigger UI updates
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        download.Status = DownloadStatus.Paused;
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to restore download {download.DownloadId}: {ex.Message}");
                    // Set status to failed if restoration fails
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        download.Status = DownloadStatus.Failed;
                        download.ErrorMessage = $"Failed to restore: {ex.Message}";
                    });
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load downloads: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SaveDownloadsAsync()
    {
        try
        {
            await _dataPersistenceService.SaveDownloadsAsync(Downloads);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save downloads: {ex.Message}");
        }
    }

    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        
        if (e.PropertyName == nameof(SelectedDownload))
        {
            // Update command can execute states
            ResumeDownloadCommand.NotifyCanExecuteChanged();
            PauseDownloadCommand.NotifyCanExecuteChanged();
            RestartDownloadCommand.NotifyCanExecuteChanged();
            RemoveDownloadCommand.NotifyCanExecuteChanged();
            ShowDownloadDetailsCommand.NotifyCanExecuteChanged();
        }
    }

    public void Cleanup()
    {
        _saveTimer?.Dispose();
        _downloadManager.ProgressChanged -= OnProgressChanged;
        _downloadManager.DownloadCompleted -= OnDownloadCompleted;
        _downloadManager.DownloadFailed -= OnDownloadFailed;
    }
}