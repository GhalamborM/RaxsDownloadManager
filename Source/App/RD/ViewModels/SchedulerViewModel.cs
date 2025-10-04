using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RD.Core.Models;
using Scheduler.Interfaces;
using Scheduler.Models;
using System.Collections.ObjectModel;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Application;
using Microsoft.Extensions.Options;
using System.IO;

namespace RD.ViewModels;

public partial class SchedulerViewModel : ObservableObject
{
    private readonly ITaskScheduler _taskScheduler;
    private readonly SchedulerOptions _options;

    [ObservableProperty]
    private ObservableCollection<ScheduledTask> tasks = [];

    [ObservableProperty]
    private ScheduledTask? selectedTask;

    [ObservableProperty]
    private ScheduledDownload? selectedDownload;

    [ObservableProperty]
    private bool isGloballyEnabled;

    [ObservableProperty]
    private int defaultMaxConcurrentDownloads;

    [ObservableProperty]
    private int checkIntervalSeconds;

    [ObservableProperty]
    private bool autoRemoveCompletedDownloads;

    [ObservableProperty]
    private bool autoRemoveCompletedTasks;

    [ObservableProperty]
    private int failedDownloadRetryDelayMinutes;

    [ObservableProperty]
    private int maxRetryAttempts;

    // Task creation properties
    [ObservableProperty]
    private string newTaskName = "";

    [ObservableProperty]
    private DateTime newTaskStartTime = DateTime.Now;

    [ObservableProperty]
    private DateTime? newTaskEndTime;

    [ObservableProperty]
    private int? newTaskMaxConcurrentDownloads;

    [ObservableProperty]
    private bool hasEndTime;

    // Download addition properties
    [ObservableProperty]
    private string newDownloadUrl = "";

    [ObservableProperty]
    private string newDownloadFilePath = "";

    public SchedulerViewModel(ITaskScheduler taskScheduler, IOptions<SchedulerOptions> options)
    {
        _taskScheduler = taskScheduler;
        _options = options.Value;

        // Initialize properties from options
        IsGloballyEnabled = _taskScheduler.IsGloballyEnabled;
        DefaultMaxConcurrentDownloads = _options.DefaultMaxConcurrentDownloads;
        CheckIntervalSeconds = (int)_options.CheckInterval.TotalSeconds;
        AutoRemoveCompletedDownloads = _options.AutoRemoveCompletedDownloads;
        AutoRemoveCompletedTasks = _options.AutoRemoveCompletedTasks;
        FailedDownloadRetryDelayMinutes = (int)_options.FailedDownloadRetryDelay.TotalMinutes;
        MaxRetryAttempts = _options.MaxRetryAttempts;

        // Subscribe to events
        _taskScheduler.TaskStarted += OnTaskStarted;
        _taskScheduler.TaskCompleted += OnTaskCompleted;
        _taskScheduler.TaskStopped += OnTaskStopped;
        _taskScheduler.TaskFailed += OnTaskFailed;

        LoadTasks();
    }

    private void LoadTasks()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Tasks.Clear();
            foreach (var task in _taskScheduler.Tasks)
            {
                Tasks.Add(task);
            }
        });
    }

    [RelayCommand]
    private void CreateTask()
    {
        try
        {
            var endTime = HasEndTime ? NewTaskEndTime : null;
            var task = _taskScheduler.CreateTask(
                string.IsNullOrWhiteSpace(NewTaskName) ? null : NewTaskName,
                NewTaskStartTime,
                endTime,
                NewTaskMaxConcurrentDownloads);

            Tasks.Add(task);

            // Reset form
            NewTaskName = "";
            NewTaskStartTime = DateTime.Now;
            NewTaskEndTime = null;
            NewTaskMaxConcurrentDownloads = null;
            HasEndTime = false;

            MessageBox.Show($"Task '{task.Name}' created successfully!", "Success", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to create task: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task StartTaskAsync()
    {
        if (SelectedTask == null) return;

        try
        {
            await _taskScheduler.StartTaskAsync(SelectedTask.Id);
            LoadTasks(); // Refresh to show updated status
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to start task: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task StopTaskAsync()
    {
        if (SelectedTask == null) return;

        try
        {
            await _taskScheduler.StopTaskAsync(SelectedTask.Id);
            LoadTasks(); // Refresh to show updated status
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to stop task: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task RemoveTaskAsync()
    {
        if (SelectedTask == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to remove task '{SelectedTask.Name}' and all its downloads?",
            "Confirm Removal", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await _taskScheduler.RemoveTaskAsync(SelectedTask.Id);
                Tasks.Remove(SelectedTask);
                SelectedTask = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to remove task: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void ToggleTaskEnabled()
    {
        if (SelectedTask == null) return;

        try
        {
            _taskScheduler.SetTaskEnabled(SelectedTask.Id, !SelectedTask.IsEnabled);
            LoadTasks(); // Refresh to show updated status
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to toggle task: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void AddDownloadToTask()
    {
        if (SelectedTask == null || string.IsNullOrWhiteSpace(NewDownloadUrl) || string.IsNullOrWhiteSpace(NewDownloadFilePath))
        {
            MessageBox.Show("Please select a task and provide both URL and file path.", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            _taskScheduler.AddDownloadToTask(SelectedTask.Id, NewDownloadUrl, NewDownloadFilePath);
            LoadTasks(); // Refresh to show the new download

            // Reset form
            NewDownloadUrl = "";
            NewDownloadFilePath = "";

            MessageBox.Show("Download added to task successfully!", "Success", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to add download: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void RemoveDownloadFromTask()
    {
        if (SelectedTask == null || SelectedDownload == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to remove this download from the task?",
            "Confirm Removal", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _taskScheduler.RemoveDownloadFromTask(SelectedTask.Id, SelectedDownload.Id);
                LoadTasks(); // Refresh to show updated downloads
                SelectedDownload = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to remove download: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void BrowseFilePath()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog();
        if (!string.IsNullOrWhiteSpace(NewDownloadUrl))
        {
            try
            {
                var uri = new Uri(NewDownloadUrl);
                var fileName = Path.GetFileName(uri.LocalPath);
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    dialog.FileName = fileName;
                }
            }
            catch { /* Ignore invalid URLs */ }
        }

        if (dialog.ShowDialog() == true)
        {
            NewDownloadFilePath = dialog.FileName;
        }
    }

    [RelayCommand]
    private void ApplyOptions()
    {
        try
        {
            // Update scheduler global state
            _taskScheduler.IsGloballyEnabled = IsGloballyEnabled;

            // Update options (these would typically be saved to configuration)
            _options.DefaultMaxConcurrentDownloads = DefaultMaxConcurrentDownloads;
            _options.CheckInterval = TimeSpan.FromSeconds(CheckIntervalSeconds);
            _options.AutoRemoveCompletedDownloads = AutoRemoveCompletedDownloads;
            _options.AutoRemoveCompletedTasks = AutoRemoveCompletedTasks;
            _options.FailedDownloadRetryDelay = TimeSpan.FromMinutes(FailedDownloadRetryDelayMinutes);
            _options.MaxRetryAttempts = MaxRetryAttempts;

            MessageBox.Show("Options applied successfully!", "Success", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to apply options: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnTaskStarted(object? sender, Scheduler.Events.TaskEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() => LoadTasks());
    }

    private void OnTaskCompleted(object? sender, Scheduler.Events.TaskEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() => LoadTasks());
    }

    private void OnTaskStopped(object? sender, Scheduler.Events.TaskEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() => LoadTasks());
    }

    private void OnTaskFailed(object? sender, Scheduler.Events.TaskErrorEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() => 
        {
            LoadTasks();
            MessageBox.Show($"Task failed: {e.Exception.Message}", "Task Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        });
    }

    public void Cleanup()
    {
        _taskScheduler.TaskStarted -= OnTaskStarted;
        _taskScheduler.TaskCompleted -= OnTaskCompleted;
        _taskScheduler.TaskStopped -= OnTaskStopped;
        _taskScheduler.TaskFailed -= OnTaskFailed;
    }
}