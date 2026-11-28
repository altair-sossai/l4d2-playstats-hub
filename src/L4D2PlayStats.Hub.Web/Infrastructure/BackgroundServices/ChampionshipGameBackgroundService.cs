using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Infrastructure.BackgroundServices;

public class ChampionshipGameBackgroundService(IServiceProvider serviceProvider, HubState hubState, ILogger<ChampionshipGameBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(AppConsts.ChampionshipGameCloseIntervalInSeconds));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = serviceProvider.CreateScope();

                await using (await hubState.LockAsync(stoppingToken))
                {
                    var closed = await scope.ServiceProvider.GetRequiredService<IChampionshipMatchService>().CloseIdleAsync(stoppingToken);

                    if (closed > 0)
                        logger.LogInformation("Ended {Games} championship games with no new statistics", closed);
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Could not end the championship games with no new statistics");
            }
        }
    }
}
