using L4D2PlayStats.Hub.Core.Modules.Seasons.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Infrastructure.BackgroundServices;

public class SeasonBackgroundService(IServiceProvider serviceProvider, ILogger<SeasonBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(AppConsts.SeasonCheckIntervalInMinutes));

        do
        {
            try
            {
                using var scope = serviceProvider.CreateScope();

                await scope.ServiceProvider.GetRequiredService<ISeasonService>().GetCurrentAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Could not ensure the current season");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
