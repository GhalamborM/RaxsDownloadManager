using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Scheduler.Interfaces;
using Scheduler.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace RD.ViewModels;

public class TaskSelectionViewModel : ObservableObject
{
    private readonly ITaskScheduler _taskScheduler;

    private string _url = "";
    public string Url
    {
        get => _url;
        set => SetProperty(ref _url, value);
    }

    private string _filePath = "";
    public string FilePath
    {
        get => _filePath;
        set => SetProperty(ref _filePath, value);
    }

    private ObservableCollection<ScheduledTask> _tasks = [];
    public ObservableCollection<ScheduledTask> Tasks
    {
        get => _tasks;
        set => SetProperty(ref _tasks, value);
    }

    private ScheduledTask? _selectedTask;
    public ScheduledTask? SelectedTask
    {
        get => _selectedTask;
        set => SetProperty(ref _selectedTask, value);
    }

    public event Action<ScheduledTask, string, string>? TaskSelected;
    public event Action? Cancelled;

    public IRelayCommand BrowseCommand { get; }
    public IRelayCommand AddToTaskCommand { get; }
    public IRelayCommand CancelCommand { get; }

    public TaskSelectionViewModel(ITaskScheduler taskScheduler)
    {
        _taskScheduler = taskScheduler;
        
        BrowseCommand = new RelayCommand(Browse);
        AddToTaskCommand = new RelayCommand(AddToTask);
        CancelCommand = new RelayCommand(Cancel);
        
        LoadTasks();
    }

    public void SetUrl(string downloadUrl)
    {
        Url = downloadUrl;
        
        // Try to generate a default file path based on the URL
        try
        {
            var uri = new Uri(downloadUrl);
            var fileName = Path.GetFileName(uri.LocalPath);
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                // Generate a default path in Downloads folder
                var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                FilePath = Path.Combine(downloadsPath, fileName);
            }
        }
        catch
        {
        }
    }

    private void LoadTasks()
    {
        Tasks.Clear();
        foreach (var task in _taskScheduler.Tasks.Where(t => t.IsEnabled))
        {
            Tasks.Add(task);
        }
    }

    private void Browse()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog();
        
        if (!string.IsNullOrWhiteSpace(FilePath))
        {
            try
            {
                dialog.InitialDirectory = Path.GetDirectoryName(FilePath);
                dialog.FileName = Path.GetFileName(FilePath);
            }
            catch { /* Ignore path parsing errors */ }
        }
        else if (!string.IsNullOrWhiteSpace(Url))
        {
            try
            {
                var uri = new Uri(Url);
                var fileName = Path.GetFileName(uri.LocalPath);
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    dialog.FileName = fileName;
                }
            }
            catch { /* Ignore URL parsing errors */ }
        }

        if (dialog.ShowDialog() == true)
        {
            FilePath = dialog.FileName;
        }
    }

    private void AddToTask()
    {
        if (SelectedTask == null)
        {
            MessageBox.Show("Please select a task.", "No Task Selected", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(FilePath))
        {
            MessageBox.Show("Please specify a file path.", "No File Path", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        TaskSelected?.Invoke(SelectedTask, Url, FilePath);
    }

    private void Cancel()
    {
        Cancelled?.Invoke();
    }
}