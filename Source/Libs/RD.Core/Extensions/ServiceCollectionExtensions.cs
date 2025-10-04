using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RD.Core.Interfaces;
using RD.Core.Models;
using RD.Core.Services;

namespace RD.Core.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the RD Download Manager services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddDownloadManager(this IServiceCollection services)
    {
        return services.AddDownloadManager(options => { });
    }

    /// <summary>
    /// Adds the RD Download Manager services to the dependency injection container with custom options
    /// </summary>
    public static IServiceCollection AddDownloadManager(this IServiceCollection services, 
        Action<DownloadManagerOptions> configureOptions)
    {
        // Configure options
        services.Configure(configureOptions);

        // Register core services
        services.TryAddSingleton<IDownloadManager, DownloadManager>();
        services.TryAddSingleton<IHttpClientFactory, HttpClientFactory>();
        services.TryAddSingleton<IDownloadInfoProvider, DownloadInfoProvider>();
        services.TryAddSingleton<ISegmentDownloader, SegmentDownloader>();
        services.TryAddSingleton<IFileMerger, FileMerger>();

        // Ensure logging is available (but don't override existing registration)
        services.TryAddSingleton(typeof(ILogger<>), typeof(Logger<>));
        services.TryAddSingleton<ILoggerFactory, LoggerFactory>();

        return services;
    }

    /// <summary>
    /// Adds the RD Download Manager as a singleton with default options
    /// </summary>
    public static IServiceCollection AddSingletonDownloadManager(this IServiceCollection services)
    {
        return services.AddDownloadManager()
                      .AddSingleton<IDownloadManager, DownloadManager>();
    }

    /// <summary>
    /// Adds the RD Download Manager as a transient service
    /// </summary>
    public static IServiceCollection AddTransientDownloadManager(this IServiceCollection services, 
        Action<DownloadManagerOptions>? configureOptions = null)
    {
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        services.TryAddTransient<IDownloadManager, DownloadManager>();
        services.TryAddSingleton<IHttpClientFactory, HttpClientFactory>();
        services.TryAddSingleton<IDownloadInfoProvider, DownloadInfoProvider>();
        services.TryAddSingleton<ISegmentDownloader, SegmentDownloader>();
        services.TryAddSingleton<IFileMerger, FileMerger>();

        services.TryAddSingleton(typeof(ILogger<>), typeof(Microsoft.Extensions.Logging.Logger<>));
        services.TryAddSingleton<ILoggerFactory, LoggerFactory>();

        return services;
    }
}