using Microsoft.Extensions.DependencyInjection;
using RD.Controls;
using RD.Core.Models;
using RD.Core.Helpers;
using RD.Models;
using RD.Services;
using RD.ViewModels;
using RD.Views;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace RD;
#pragma warning disable
public partial class MainWindow : Window
{
    private readonly IDataPersistenceService _dataPersistenceService;
    private bool _isLoaded = false;
    private DispatcherTimer? _saveTimer;
    private NotifyIcon? _notifyIcon;
    private bool _isExiting = false;
    private string? _lastClipboardContent;
    private bool _isClipboardMonitoringEnabled = false;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr AddClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

    private const int WM_CLIPBOARDUPDATE = 0x031D;

    public MainWindow()
    {
        InitializeComponent();
        _dataPersistenceService = App.ServiceProvider.GetRequiredService<IDataPersistenceService>();
        DataContext = App.ServiceProvider.GetRequiredService<MainViewModel>();
        
        InitializeNotifyIcon();
        
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
        SizeChanged += MainWindow_SizeChanged;
    }

    private void InitializeNotifyIcon()
    {
        _notifyIcon = new NotifyIcon
        {
            Icon = new Icon("RDLogo.ico"),
            Visible = true,
            Text = "Raxs Download Manager"
        };

        _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Show", null, (s, e) => ShowWindow());
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("Exit", null, (s, e) => ExitApplication());
        
        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
    {
        ShowWindow();
    }

    private void ShowWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        _notifyIcon!.Visible = false;
    }

    private void ExitApplication()
    {
        _isExiting = true;
        System.Windows.Application.Current.Shutdown();
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = true;
        await LoadUISettingsAsync();
        await InitializeClipboardMonitoringAsync();
        
        if (DownloadsDataGrid != null)
        {
            DownloadsDataGrid.LayoutUpdated += DataGrid_LayoutUpdated;
        }
    }

    private async Task InitializeClipboardMonitoringAsync()
    {
        try
        {
            var options = await _dataPersistenceService.LoadOptionsAsync();
            _isClipboardMonitoringEnabled = options.MonitorClipboardForUrls;

            if (_isClipboardMonitoringEnabled)
            {
                _lastClipboardContent = GetClipboardText();

                var windowHandle = new WindowInteropHelper(this).Handle;
                var source = HwndSource.FromHwnd(windowHandle);
                source?.AddHook(WndProc);
                AddClipboardFormatListener(windowHandle);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to initialize clipboard monitoring: {ex.Message}");
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_CLIPBOARDUPDATE && _isClipboardMonitoringEnabled)
        {
            CheckClipboardForUrl();
        }
        return IntPtr.Zero;
    }

    private void CheckClipboardForUrl()
    {
        try
        {
            var clipboardText = GetClipboardText();

            if (string.IsNullOrWhiteSpace(clipboardText) || clipboardText == _lastClipboardContent)
                return;

            _lastClipboardContent = clipboardText;

            if (Helper.IsValidUrl(clipboardText))
            {
                Dispatcher.InvokeAsync(() =>
                {
                    ShowAddDownloadWindowWithUrl(clipboardText);
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking clipboard: {ex.Message}");
        }
    }

    private string? GetClipboardText()
    {
        try
        {
            if (System.Windows.Clipboard.ContainsText())
            {
                return System.Windows.Clipboard.GetText();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting clipboard text: {ex.Message}");
        }
        return null;
    }

    private void ShowAddDownloadWindowWithUrl(string url)
    {
        //if (!IsVisible || WindowState == WindowState.Minimized)
        //{
        //    Show();
        //    WindowState = WindowState.Normal;
        //    Activate();
        //    _notifyIcon!.Visible = false;
        //}

        var addDownloadWindow = App.ServiceProvider.GetRequiredService<AddDownloadWindow>();
        var viewModel = App.ServiceProvider.GetRequiredService<AddDownloadViewModel>();
        addDownloadWindow.DataContext = viewModel;
        
        viewModel.Url = url;
        
        viewModel.DownloadAdded += async (downloadUrl, options) =>
        {
            if (DataContext is MainViewModel mainViewModel)
            {
                await mainViewModel.AddDownloadAsync(downloadUrl, options);
            }
            addDownloadWindow.Close();
        };

        addDownloadWindow.Owner = this;
        addDownloadWindow.ShowDialog();
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
        if (!_isExiting)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        _saveTimer?.Stop();
        
        try
        {
            var windowHandle = new WindowInteropHelper(this).Handle;
            RemoveClipboardFormatListener(windowHandle);
        }
        catch { }
        
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

        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }
    }
}