using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WTW.MdpService.BackgroundTasks;

public class BackgroundTasksService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<BackgroundTasksService> _logger;

    public BackgroundTasksService(
        IServiceProvider services,
        ILogger<BackgroundTasksService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _services.CreateScope();
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var task in scope.ServiceProvider.GetServices<IBackgroundTask>())
            {
                try
                {
                    await task.Start(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Background task {task.GetType().Name} failed");
                }
            }

            await Task.Delay(30000, stoppingToken);
        }
    }
}