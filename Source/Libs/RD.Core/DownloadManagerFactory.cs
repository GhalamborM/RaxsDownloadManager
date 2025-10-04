using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RD.Core.Extensions;
using RD.Core.Interfaces;
using RD.Core.Models;

namespace RD.Core;

/// <summary>
/// Factory class for creating download manager instances without dependency injection
/// </summary>
public static class DownloadManagerFactory
{
    /// <summary>
    /// Creates a new download manager instance with default options
    /// </summary>
    public static IDownloadManager Create()
    {
        return Create(new DownloadManagerOptions());
    }

    /// <summary>
    /// Creates a new download manager instance with custom options
    /// </summary>
    public static IDownloadManager Create(DownloadManagerOptions options)
    {
        var services = new ServiceCollection();

        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddDownloadManager(opt =>
        {
            opt.MaxConcurrentDownloads = options.MaxConcurrentDownloads;
            opt.TempDirectory = options.TempDirectory;
            opt.DefaultMaxSegments = options.DefaultMaxSegments;
            opt.DefaultBufferSize = options.DefaultBufferSize;
            opt.DefaultTimeout = options.DefaultTimeout;
            opt.DefaultRetryAttempts = options.DefaultRetryAttempts;
            opt.DefaultRetryDelay = options.DefaultRetryDelay;
            opt.ProgressReportInterval = options.ProgressReportInterval;
            opt.CleanupTempFilesOnSuccess = options.CleanupTempFilesOnSuccess;
            opt.CleanupTempFilesOnFailure = options.CleanupTempFilesOnFailure;
        });

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IDownloadManager>();
    }

    /// <summary>
    /// Creates a new download manager instance with configuration action
    /// </summary>
    public static IDownloadManager Create(Action<DownloadManagerOptions> configure)
    {
        var options = new DownloadManagerOptions();
        configure(options);
        return Create(options);
    }

    /// <summary>
    /// Creates a new download manager instance with custom logging
    /// </summary>
    public static IDownloadManager Create(DownloadManagerOptions options, ILoggerFactory loggerFactory)
    {
        var services = new ServiceCollection();
        
        services.AddSingleton(loggerFactory);
        services.AddDownloadManager(opt =>
        {
            opt.MaxConcurrentDownloads = options.MaxConcurrentDownloads;
            opt.TempDirectory = options.TempDirectory;
            opt.DefaultMaxSegments = options.DefaultMaxSegments;
            opt.DefaultBufferSize = options.DefaultBufferSize;
            opt.DefaultTimeout = options.DefaultTimeout;
            opt.DefaultRetryAttempts = options.DefaultRetryAttempts;
            opt.DefaultRetryDelay = options.DefaultRetryDelay;
            opt.ProgressReportInterval = options.ProgressReportInterval;
            opt.CleanupTempFilesOnSuccess = options.CleanupTempFilesOnSuccess;
            opt.CleanupTempFilesOnFailure = options.CleanupTempFilesOnFailure;
        });

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IDownloadManager>();
    }

    /// <summary>
    /// Creates a simple download manager with minimal logging for basic usage
    /// </summary>
    public static IDownloadManager CreateSimple()
    {
        var services = new ServiceCollection();
        
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
        services.AddDownloadManager();

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IDownloadManager>();
    }
}