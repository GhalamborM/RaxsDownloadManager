using Microsoft.Extensions.DependencyInjection;
using RD.Controls;
using RD.Core.Models;
using RD.Models;
using RD.Services;
using RD.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RD;

public partial class MainWindow : Window
{
    private readonly IDataPersistenceService _dataPersistenceService;
    private bool _isLoaded = false;
    private DispatcherTimer? _saveTimer;

    public MainWindow()
    {
        InitializeComponent();
        _dataPersistenceService = App.ServiceProvider.GetRequiredService<IDataPersistenceService>();
        DataContext = App.ServiceProvider.GetRequiredService<MainViewModel>();
        
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
        SizeChanged += MainWindow_SizeChanged;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = true;
        await LoadUISettingsAsync();
        
        if (DownloadsDataGrid != null)
        {
            DownloadsDataGrid.LayoutUpdated += DataGrid_LayoutUpdated;
        }
    }

    private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_isLoaded)
        {
            DebounceSave();
        }
    }

    private void DataGrid_LayoutUpdated(object? sender, EventArgs e)
    {
        if (_isLoaded)
        {
            DebounceSave();
        }
    }

    private void DebounceSave()
    {
        _saveTimer?.Stop();
        _saveTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _saveTimer.Tick += async (s, e) =>
        {
            _saveTimer?.Stop();
            await SaveUISettingsAsync();
        };
        _saveTimer.Start();
    }

    private async Task LoadUISettingsAsync()
    {
        try
        {
            var settings = await _dataPersistenceService.LoadUISettingsAsync();
            
            if (settings.MainWindow != null)
            {
                if (settings.MainWindow.Width > 0)
                    Width = settings.MainWindow.Width;
                
                if (settings.MainWindow.Height > 0)
                    Height = settings.MainWindow.Height;
                
                if (!double.IsNaN(settings.MainWindow.Left) && settings.MainWindow.Left >= SystemParameters.VirtualScreenLeft)
                    Left = settings.MainWindow.Left;
                
                if (!double.IsNaN(settings.MainWindow.Top) && settings.MainWindow.Top >= SystemParameters.VirtualScreenTop)
                    Top = settings.MainWindow.Top;
                
                if (settings.MainWindow.IsMaximized)
                    WindowState = WindowState.Maximized;
            }
            
            if (DownloadsDataGrid != null && settings.DataGridColumnWidths.Count > 0)
            {
                foreach (var column in DownloadsDataGrid.Columns)
                {
                    var header = column.Header?.ToString() ?? string.Empty;
                    if (settings.DataGridColumnWidths.TryGetValue(header, out var width) && width > 0)
                    {
                        column.Width = new DataGridLength(width);
                    }
                }
            }
            
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.IsCategoryPanelCollapsed = settings.IsCategoryPanelCollapsed;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load UI settings: {ex.Message}");
        }
    }

    private async Task SaveUISettingsAsync()
    {
        try
        {
            var settings = new UISettings
            {
                MainWindow = new WindowSettings
                {
                    Width = WindowState == WindowState.Normal ? Width : RestoreBounds.Width,
                    Height = WindowState == WindowState.Normal ? Height : RestoreBounds.Height,
                    Left = WindowState == WindowState.Normal ? Left : RestoreBounds.Left,
                    Top = WindowState == WindowState.Normal ? Top : RestoreBounds.Top,
                    IsMaximized = WindowState == WindowState.Maximized
                },
                DataGridColumnWidths = []
            };
            
            if (DownloadsDataGrid != null)
            {
                foreach (var column in DownloadsDataGrid.Columns)
                {
                    var header = column.Header?.ToString() ?? string.Empty;
                    if (!string.IsNullOrEmpty(header))
                    {
                        settings.DataGridColumnWidths[header] = column.ActualWidth;
                    }
                }
            }
            
            if (DataContext is MainViewModel viewModel)
            {
                settings.IsCategoryPanelCollapsed = viewModel.IsCategoryPanelCollapsed;
            }
            
            await _dataPersistenceService.SaveUISettingsAsync(settings);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save UI settings: {ex.Message}");
        }
    }

    private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
    {
        if (sender is TreeViewItem item && item.DataContext is CategoryTreeNode node)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.SelectedCategoryNode = node;
            }
            e.Handled = true;
        }
    }

    private void ToggleCategoryPanel_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.IsCategoryPanelCollapsed = !viewModel.IsCategoryPanelCollapsed;
            _ = SaveUISettingsAsync();
        }
    }

    private async void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        _saveTimer?.Stop();
        await SaveUISettingsAsync();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        
        if (DownloadsDataGrid != null)
        {
            DownloadsDataGrid.LayoutUpdated -= DataGrid_LayoutUpdated;
        }
        
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.Cleanup();
        }
    }
}