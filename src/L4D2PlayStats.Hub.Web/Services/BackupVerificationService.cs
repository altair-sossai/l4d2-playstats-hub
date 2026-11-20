using System.Diagnostics;
using System.Globalization;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Backups.Enums;
using L4D2PlayStats.Hub.Core.Modules.Backups.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Backups.Models;
using L4D2PlayStats.Hub.Core.Modules.Backups.Settings;
using L4D2PlayStats.Hub.Web.Infrastructure.Backups;
using L4D2PlayStats.Hub.Web.Models.Backups;
using Microsoft.EntityFrameworkCore;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Backups.Contracts;
using L4D2PlayStats.Hub.Web.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Services;

public class BackupVerificationService(HubDbContext dbContext, IClock clock, ILogger<BackupVerificationService> logger) : IBackupVerificationService
{
    public bool IsAvailable => dbContext.Database.IsSqlite();

    public BackupVerification NotAvailable()
    {
        var verification = BackupVerification.Of(BackupVerificationStatus.NotAvailable, AppConsts.BackupVerifyNotAvailableMessage);

        verification.VerifiedAtUtc = clock.UtcNow;

        return verification;
    }

    public async Task<BackupVerification> VerifyAsync(IBackupStorage storage, BackupSettings settings, string blobName, long? expectedSize, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return NotAvailable();

        var stopwatch = Stopwatch.StartNew();
        var work = Path.Combine(Path.GetTempPath(), AppConsts.BackupTempFolder, AppConsts.BackupVerifyFolderPrefix + Guid.NewGuid().ToString("N"));
        BackupVerification verification;

        try
        {
            Directory.CreateDirectory(work);

            var zipPath = Path.Combine(work, AppConsts.BackupVerifyZipName);

            await using (var file = new FileStream(zipPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, AppConsts.BackupVerifyBufferSize, true))
                await storage.DownloadAsync(blobName, file, cancellationToken);

            verification = await BackupArchiveVerifier.VerifyAsync(zipPath, work, BackupTables.From(dbContext.Model), expectedSize, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            verification = BackupVerification.Of(BackupVerificationStatus.Failed, AppConsts.BackupVerifyUnreadableMessage, BackupErrors.Describe(exception, settings));
        }
        finally
        {
            Clean(work);
        }

        verification.VerifiedAtUtc = clock.UtcNow;
        verification.DurationInMilliseconds = stopwatch.ElapsedMilliseconds;

        Log(blobName, verification, settings);

        return verification;
    }

    private void Log(string blobName, BackupVerification verification, BackupSettings settings)
    {
        var arguments = verification.Arguments.Cast<object>().ToArray();
        var text = BackupErrors.Limit(BackupSecrets.Redact(string.Format(CultureInfo.InvariantCulture, verification.Message, arguments), settings));

        if (verification.Status == BackupVerificationStatus.Failed)
            logger.LogError("Verification of backup {Name} failed: {Error}", blobName, text);
        else
            logger.LogInformation("Backup {Name} verified in {Duration} ms: {Result}", blobName, verification.DurationInMilliseconds, text);
    }

    private void Clean(string work)
    {
        try
        {
            if (Directory.Exists(work))
                Directory.Delete(work, true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            logger.LogWarning("Could not delete the backup verification folder: {Error}", exception.GetType().Name);
        }
    }
}
