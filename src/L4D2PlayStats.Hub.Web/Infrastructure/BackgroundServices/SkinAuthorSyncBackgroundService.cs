using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Web.Services;

namespace L4D2PlayStats.Hub.Web.Infrastructure.BackgroundServices;

public class SkinAuthorSyncBackgroundService(IServiceProvider serviceProvider, ILogger<SkinAuthorSyncBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(AppConsts.SkinAuthorSyncStartDelayInSeconds), stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(AppConsts.SkinAuthorCheckIntervalInMinutes));

        do
        {
            try
            {
                await RunAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(exception, "Could not sync the skin authors with Steam");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    internal async Task RunAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        var modules = await scope.ServiceProvider.GetRequiredService<ISettingsService>().GetAsync<ModuleSettings>(cancellationToken);
        if (!modules.IsEnabled(HubModule.SkinShop))
            return;

        await scope.ServiceProvider.GetRequiredService<ISkinAuthorSyncService>().SyncAsync(false, cancellationToken);
    }
}
