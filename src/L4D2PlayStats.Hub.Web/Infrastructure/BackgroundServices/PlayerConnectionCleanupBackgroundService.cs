using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Infrastructure.BackgroundServices;

public class PlayerConnectionCleanupBackgroundService(IServiceProvider serviceProvider, ILogger<PlayerConnectionCleanupBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(AppConsts.PlayerConnectionCleanupIntervalInHours));

        do
        {
            try
            {
                using var scope = serviceProvider.CreateScope();

                var deleted = await scope.ServiceProvider.GetRequiredService<IPlayerConnectionService>().CleanupAsync(stoppingToken);

                if (deleted > 0)
                    logger.LogInformation("Deleted {Connections} old player connections", deleted);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Could not delete the old player connections");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}