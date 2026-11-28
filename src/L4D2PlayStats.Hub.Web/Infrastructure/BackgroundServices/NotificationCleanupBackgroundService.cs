using L4D2PlayStats.Hub.Core.Modules.Notifications.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Infrastructure.BackgroundServices;

public class NotificationCleanupBackgroundService(IServiceProvider serviceProvider, ILogger<NotificationCleanupBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(AppConsts.NotificationCleanupIntervalInHours));

        do
        {
            try
            {
                using var scope = serviceProvider.CreateScope();

                var deleted = await scope.ServiceProvider.GetRequiredService<INotificationService>().CleanupAsync(stoppingToken);

                if (deleted > 0)
                    logger.LogInformation("Deleted {Notifications} old notifications", deleted);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Could not delete the old notifications");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
