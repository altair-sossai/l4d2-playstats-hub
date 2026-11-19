using System.Diagnostics;
using System.Globalization;
using System.Text;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Backups.Enums;
using L4D2PlayStats.Hub.Core.Modules.Backups.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Backups.Models;
using L4D2PlayStats.Hub.Core.Modules.Backups.Services;
using L4D2PlayStats.Hub.Core.Modules.Backups.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Web.Infrastructure.Backups;
using L4D2PlayStats.Hub.Web.Models.Backups;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Services;

public class BackupService(
    ISettingsService settingsService,
    IBackupHistoryService historyService,
    IBackupArchiveService archiveService,
    IBackupStorageFactory storageFactory,
    BackupLock backupLock,
    IClock clock,
    ILogger<BackupService> logger) : IBackupService
{
    public bool IsRunning => backupLock.IsRunning;

    public async Task<BackupRun?> RunIfDueAsync(CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.GetAsync<BackupSettings>(cancellationToken);
        var history = await historyService.GetAsync(cancellationToken);

        return BackupSchedule.IsDue(settings, history, clock.UtcNow) ? await RunAsync(BackupTrigger.Schedule, cancellationToken) : null;
    }

    public async Task<BackupRun?> RunAsync(BackupTrigger trigger, CancellationToken cancellationToken = default)
    {
        if (!backupLock.TryEnter())
            return null;

        try
        {
            var settings = await settingsService.GetAsync<BackupSettings>(cancellationToken);
            var run = await ExecuteAsync(settings, trigger, cancellationToken);

            await historyService.AddAsync(run, cancellationToken);

            return run;
        }
        finally
        {
            backupLock.Exit();
        }
    }

    public async Task<BackupCheck> TestAsync(CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.GetAsync<BackupSettings>(cancellationToken);

        if (!BackupRules.IsConfigured(settings))
            return new BackupCheck(false, AppConsts.BackupNotConfiguredMessage);

        var prefix = Prefix(settings);
        var name = string.Format(CultureInfo.InvariantCulture, AppConsts.BackupTestBlobFormat, prefix, Guid.NewGuid().ToString("N"));

        try
        {
            var storage = storageFactory.Create(settings);

            await storage.PrepareAsync(cancellationToken);

            await using (var content = new MemoryStream(Encoding.UTF8.GetBytes(AppConsts.BackupTestContent)))
                await storage.UploadAsync(name, content, AppConsts.BackupTestContentType, cancellationToken);

            var listed = (await storage.ListAsync(prefix, cancellationToken)).Contains(name);

            await storage.DeleteAsync(name, cancellationToken);

            return listed ? new BackupCheck(true, null) : new BackupCheck(false, AppConsts.BackupTestFailedMessage);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            var error = Describe(exception, settings);

            logger.LogWarning("The backup destination test failed: {Error}", error);

            return new BackupCheck(false, error);
        }
    }

    private async Task<BackupRun> ExecuteAsync(BackupSettings settings, BackupTrigger trigger, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var startedAt = clock.UtcNow;
        var name = BackupNames.FileName(Prefix(settings), startedAt);
        var run = new BackupRun { StartedAtUtc = startedAt, Trigger = trigger, BlobName = name };
        var zipPath = Path.Combine(Path.GetTempPath(), AppConsts.BackupTempFolder, name);

        try
        {
            if (!BackupRules.IsConfigured(settings))
                throw new InvalidOperationException(AppConsts.BackupNotConfiguredMessage);

            Directory.CreateDirectory(Path.GetDirectoryName(zipPath)!);

            await archiveService.CreateAsync(zipPath, cancellationToken);

            run.SizeInBytes = new FileInfo(zipPath).Length;

            var storage = storageFactory.Create(settings);

            await storage.PrepareAsync(cancellationToken);

            await using (var content = File.OpenRead(zipPath))
                await storage.UploadAsync(name, content, AppConsts.BackupContentType, cancellationToken);

            run.Succeeded = true;

            await DeleteExpiredAsync(storage, settings, cancellationToken);

            logger.LogInformation("Backup {Name} uploaded, {Size} bytes", name, run.SizeInBytes);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            run.Error = Describe(exception, settings);
            logger.LogError("Backup {Name} failed: {Error}", name, run.Error);
        }
        finally
        {
            if (File.Exists(zipPath))
                File.Delete(zipPath);
        }

        run.DurationInMilliseconds = stopwatch.ElapsedMilliseconds;

        return run;
    }

    private async Task DeleteExpiredAsync(IBackupStorage storage, BackupSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            var prefix = Prefix(settings);
            var expired = BackupNames.Expired(prefix, await storage.ListAsync(prefix, cancellationToken), settings.Keep);

            foreach (var name in expired)
                await storage.DeleteAsync(name, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning("Could not delete old backups: {Error}", Describe(exception, settings));
        }
    }

    private static string Prefix(BackupSettings settings)
    {
        return BackupRules.IsValidPrefix(settings.Prefix) ? settings.Prefix : CoreConsts.BackupDefaultPrefix;
    }

    private static string Describe(Exception exception, BackupSettings settings)
    {
        var firstLine = exception.Message.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim() ?? exception.GetType().Name;
        var redacted = BackupSecrets.Redact(firstLine, settings);

        return redacted.Length <= AppConsts.BackupErrorMaxLength ? redacted : redacted[..AppConsts.BackupErrorMaxLength];
    }
}
