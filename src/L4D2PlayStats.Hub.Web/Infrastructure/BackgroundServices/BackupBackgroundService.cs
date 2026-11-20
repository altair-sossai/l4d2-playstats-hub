using L4D2PlayStats.Hub.Web.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Infrastructure.BackgroundServices;

public class BackupBackgroundService(IServiceProvider serviceProvider, ILogger<BackupBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(AppConsts.BackupCheckIntervalInMinutes));

        while (await timer.WaitForNextTickAsync(stoppingToken))
            try
            {
                using var scope = serviceProvider.CreateScope();

                await scope.ServiceProvider.GetRequiredService<IBackupService>().RunIfDueAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError("Could not check whether a backup is due: {Error}", exception.GetType().Name);
            }
    }
}
