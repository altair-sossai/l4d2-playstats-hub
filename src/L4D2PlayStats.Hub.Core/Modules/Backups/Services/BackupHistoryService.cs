using L4D2PlayStats.Hub.Core.Modules.Backups.Models;
using L4D2PlayStats.Hub.Core.Modules.Backups.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;

namespace L4D2PlayStats.Hub.Core.Modules.Backups.Services;

public class BackupHistoryService(ISettingsService settingsService) : IBackupHistoryService
{
    public Task<BackupHistory> GetAsync(CancellationToken cancellationToken = default)
    {
        return settingsService.GetAsync<BackupHistory>(cancellationToken);
    }

    public async Task AddAsync(BackupRun run, CancellationToken cancellationToken = default)
    {
        var history = await settingsService.GetAsync<BackupHistory>(cancellationToken);

        history.Runs = history.Runs
            .Append(run)
            .OrderByDescending(item => item.StartedAtUtc)
            .Take(AppConsts.BackupHistorySize)
            .ToList();

        await settingsService.SaveAsync(history, cancellationToken);
    }
}
