using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scheduler.Interfaces;
using Scheduler.Models;
using Scheduler.Services;

namespace Scheduler.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Task Scheduler services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Optional configuration for scheduler options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddTaskScheduler(
        this IServiceCollection services,
        Action<SchedulerOptions>? configureOptions = null)
    {
        // Configure options
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<SchedulerOptions>(options => { });
        }

        // Register the scheduler as singleton - use the renamed class
        services.AddSingleton<ITaskScheduler, DownloadTaskScheduler>();

        return services;
    }

    /// <summary>
    /// Adds both Download Manager and Task Scheduler services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureSchedulerOptions">Optional configuration for scheduler options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddDownloadScheduler(
        this IServiceCollection services,
        Action<SchedulerOptions>? configureSchedulerOptions = null)
    {
        // Add logging if not already added
        services.AddLogging(builder => builder.AddConsole());

        // Add the Task Scheduler (assumes Download Manager is already registered)
        services.AddTaskScheduler(configureSchedulerOptions);

        return services;
    }
}