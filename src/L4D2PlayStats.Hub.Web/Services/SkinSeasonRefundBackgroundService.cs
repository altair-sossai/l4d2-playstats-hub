using L4D2PlayStats.Hub.Core.Modules.Skins.Services;

namespace L4D2PlayStats.Hub.Web.Services;

public class SkinSeasonRefundBackgroundService(IServiceProvider serviceProvider, ILogger<SkinSeasonRefundBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(AppConsts.SkinSeasonRefundStartDelayInSeconds), stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(AppConsts.SkinSeasonRefundIntervalInMinutes));

        do
        {
            try
            {
                using var scope = serviceProvider.CreateScope();

                await scope.ServiceProvider.GetRequiredService<ISkinSeasonRefundService>().RunAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Could not refund the skin purchases of the previous season");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
