using L4D2PlayStats.Hub.Core.Modules.Backups.Models;
using L4D2PlayStats.Hub.Core.Modules.Backups.Settings;
using L4D2PlayStats.Hub.Web.Infrastructure.Backups;

namespace L4D2PlayStats.Hub.Web.Services;

public interface IBackupVerificationService
{
    bool IsAvailable { get; }
    BackupVerification NotAvailable();
    Task<BackupVerification> VerifyAsync(IBackupStorage storage, BackupSettings settings, string blobName, long? expectedSize, CancellationToken cancellationToken = default);
}
