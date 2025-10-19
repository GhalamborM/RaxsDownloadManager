using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RD.Core.Extensions;
using RD.Core.Interfaces;
using RD.Core.Models;
using Scheduler.Extensions;
using System.Windows;
using RD.Services;
using RD.ViewModels;
using RD.Views;
using ModernWpf;

namespace RD;

public partial class App : System.Windows.Application
{
    private IHost? _host;
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        // Initialize Modern WPF
        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
        ThemeManager.Current.AccentColor = System.Windows.Media.Color.FromRgb(0, 120, 212); // Windows 11 accent color

        var builder = Host.CreateApplicationBuilder();

        // Configure logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        // Configure services
        ConfigureServices(builder.Services);

        _host = builder.Build();
        ServiceProvider = _host.Services;

        // Start the host
        await _host.StartAsync();

        // Apply startup setting from saved options
        await ApplyStartupSettingAsync();

        base.OnStartup(e);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register RD library services
        services.AddDownloadManager(options =>
        {
            // Load options from settings - for now use defaults
            // In a real app, you'd load from the persistence service
            options.MaxConcurrentDownloads = 3;
            options.DefaultMaxSegments = 8;
            options.DefaultBufferSize = 8192;
            options.DefaultTimeout = TimeSpan.FromMinutes(5);
            options.DefaultRetryAttempts = 3;
            options.DefaultRetryDelay = TimeSpan.FromSeconds(2);
            options.ProgressReportInterval = TimeSpan.FromMilliseconds(500);
            options.CleanupTempFilesOnSuccess = true;
            options.CleanupTempFilesOnFailure = false;
        });

        // Register Scheduler services
        services.AddDownloadScheduler(options =>
        {
            options.IsGloballyEnabled = true;
            options.DefaultMaxConcurrentDownloads = 3;
            options.CheckInterval = TimeSpan.FromSeconds(30);
            options.AutoRemoveCompletedDownloads = true;
            options.AutoRemoveCompletedTasks = false;
            options.FailedDownloadRetryDelay = TimeSpan.FromMinutes(5);
            options.MaxRetryAttempts = 3;
        });

        // Register WPF-specific services
        services.AddSingleton<IDataPersistenceService, DataPersistenceService>();
        services.AddSingleton<IUpdateService, UpdateService>();

        // Register ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<AddDownloadViewModel>();
        services.AddTransient<OptionsViewModel>();
        services.AddTransient<DownloadDetailsViewModel>();
        services.AddTransient<SchedulerViewModel>();
        services.AddTransient<TaskSelectionViewModel>();
        services.AddTransient<UpdateViewModel>();
        services.AddTransient<EditCategoryViewModel>();

        // Register Views
        services.AddTransient<MainWindow>();
        services.AddTransient<AddDownloadWindow>();
        services.AddTransient<OptionsWindow>();
        services.AddTransient<DownloadDetailsWindow>();
        services.AddTransient<SchedulerWindow>();
        services.AddTransient<TaskSelectionWindow>();
        services.AddTransient<UpdateWindow>();
        services.AddTransient<EditCategoryWindow>();
    }

    private async Task ApplyStartupSettingAsync()
    {
        try
        {
            var dataPersistenceService = ServiceProvider.GetRequiredService<IDataPersistenceService>();
            var options = await dataPersistenceService.LoadOptionsAsync();
            
            // Sync the actual startup status with the saved setting
            StartupManager.SetStartup(options.RunAtStartup);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to apply startup setting: {ex.Message}");
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
