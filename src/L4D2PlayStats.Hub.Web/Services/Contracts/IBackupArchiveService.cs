using L4D2PlayStats.Hub.Web.Models.Backups;

namespace L4D2PlayStats.Hub.Web.Services.Contracts;

public interface IBackupArchiveService
{
    bool IncludesDatabase { get; }
    Task<BackupManifest> CreateAsync(string zipPath, CancellationToken cancellationToken = default);
}