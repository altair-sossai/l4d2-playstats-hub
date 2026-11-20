using L4D2PlayStats.Hub.Core.Modules.Backups.Models;
using L4D2PlayStats.Hub.Core.Modules.Backups.Settings;

namespace L4D2PlayStats.Hub.Core.Modules.Backups.Services.Contracts;

public interface IBackupHistoryService
{
    Task<BackupHistory> GetAsync(CancellationToken cancellationToken = default);
    Task AddAsync(BackupRun run, CancellationToken cancellationToken = default);
    Task<bool> RecordVerificationAsync(string blobName, BackupVerification verification, CancellationToken cancellationToken = default);
}