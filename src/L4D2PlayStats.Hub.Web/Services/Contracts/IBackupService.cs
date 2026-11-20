using L4D2PlayStats.Hub.Core.Modules.Backups.Enums;
using L4D2PlayStats.Hub.Core.Modules.Backups.Models;
using L4D2PlayStats.Hub.Web.Models.Backups;

namespace L4D2PlayStats.Hub.Web.Services.Contracts;

public interface IBackupService
{
    bool IsRunning { get; }
    Task<BackupRun?> RunAsync(BackupTrigger trigger, CancellationToken cancellationToken = default);
    Task<BackupRun?> RunIfDueAsync(CancellationToken cancellationToken = default);
    Task<BackupCheck> TestAsync(CancellationToken cancellationToken = default);
    Task<BackupVerifyResult?> VerifyLatestAsync(CancellationToken cancellationToken = default);
}