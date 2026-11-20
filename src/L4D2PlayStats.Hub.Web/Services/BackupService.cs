using System.Diagnostics;
using System.Globalization;
using System.Text;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Backups.Enums;
using L4D2PlayStats.Hub.Core.Modules.Backups.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Backups.Models;
using L4D2PlayStats.Hub.Core.Modules.Backups.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Backups.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Backups;
using L4D2PlayStats.Hub.Web.Infrastructure.Backups.Contracts;
using L4D2PlayStats.Hub.Web.Models.Backups;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Services;

public class BackupService(
    ISettingsService settingsService,
    IBackupHistoryService historyService,
    IBackupArchiveService archiveService,
    IBackupVerificationService verificationService,
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

    public async Task<BackupVerifyResult?> VerifyLatestAsync(CancellationToken cancellationToken = default)
    {
        if (!backupLock.TryEnter())
            return null;

        try
        {
            var settings = await settingsService.GetAsync<BackupSettings>(cancellationToken);

            return await VerifyNewestAsync(settings, cancellationToken);
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
            {
                await storage.UploadAsync(name, content, AppConsts.BackupTestContentType, cancellationToken);
            }

            var listed = (await storage.ListAsync(prefix, cancellationToken)).Contains(name);

            await storage.DeleteAsync(name, cancellationToken);

            return listed ? new BackupCheck(true, null) : new BackupCheck(false, AppConsts.BackupTestFailedMessage);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            var error = BackupErrors.Describe(exception, settings);

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
            {
                await storage.UploadAsync(name, content, AppConsts.BackupContentType, cancellationToken);
            }

            run.Succeeded = true;

            File.Delete(zipPath);

            run.Verification = await verificationService.VerifyAsync(storage, settings, name, run.SizeInBytes, cancellationToken);

            if (run.Verification.Status != BackupVerificationStatus.Failed)
                await DeleteExpiredAsync(storage, settings, cancellationToken);

            logger.LogInformation("Backup {Name} uploaded, {Size} bytes", name, run.SizeInBytes);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            run.Error = BackupErrors.Describe(exception, settings);
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

    private async Task<BackupVerifyResult> VerifyNewestAsync(BackupSettings settings, CancellationToken cancellationToken)
    {
        if (!BackupRules.IsConfigured(settings))
            return new BackupVerifyResult(null, BackupVerification.Of(BackupVerificationStatus.Failed, AppConsts.BackupNotConfiguredMessage));

        if (!verificationService.IsAvailable)
            return new BackupVerifyResult(null, verificationService.NotAvailable());

        try
        {
            var storage = storageFactory.Create(settings);
            var name = await NewestAsync(storage, settings, cancellationToken);

            if (name == null)
                return new BackupVerifyResult(null, BackupVerification.Of(BackupVerificationStatus.Failed, AppConsts.BackupVerifyNoBackupMessage));

            var history = await historyService.GetAsync(cancellationToken);
            var size = history.Runs.FirstOrDefault(run => run.BlobName == name && run.Succeeded)?.SizeInBytes;
            var verification = await verificationService.VerifyAsync(storage, settings, name, size, cancellationToken);

            await historyService.RecordVerificationAsync(name, verification, cancellationToken);

            return new BackupVerifyResult(name, verification);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            var error = BackupErrors.Describe(exception, settings);

            logger.LogError("The backup verification failed: {Error}", error);

            return new BackupVerifyResult(null, BackupVerification.Of(BackupVerificationStatus.Failed, AppConsts.BackupVerifyUnreadableMessage, error));
        }
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
            logger.LogWarning("Could not delete old backups: {Error}", BackupErrors.Describe(exception, settings));
        }
    }

    private static string Prefix(BackupSettings settings)
    {
        return BackupRules.IsValidPrefix(settings.Prefix) ? settings.Prefix : CoreConsts.BackupDefaultPrefix;
    }

    private static async Task<string?> NewestAsync(IBackupStorage storage, BackupSettings settings, CancellationToken cancellationToken)
    {
        var prefix = Prefix(settings);

        return (await storage.ListAsync(prefix, cancellationToken))
            .Where(name => BackupNames.IsBackup(prefix, name))
            .OrderByDescending(name => name, StringComparer.Ordinal)
            .FirstOrDefault();
    }
}