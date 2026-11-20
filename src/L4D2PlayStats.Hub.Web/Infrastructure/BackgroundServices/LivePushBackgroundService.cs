using L4D2PlayStats.Hub.Web.Infrastructure.Live;

namespace L4D2PlayStats.Hub.Web.Infrastructure.BackgroundServices;

public class LivePushBackgroundService(LiveNotifier notifier, ILogger<LivePushBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(AppConsts.LivePushIntervalInMilliseconds));

        while (await timer.WaitForNextTickAsync(stoppingToken))
            try
            {
                await notifier.NotifyAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(exception, "Could not notify the live server pages");
            }
    }
}
