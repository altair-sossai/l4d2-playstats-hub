using L4D2PlayStats.Hub.Core.Modules.Backups.Settings;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Backups.Contracts;

public interface IBackupStorageFactory
{
    IBackupStorage Create(BackupSettings settings);
    bool IsValidConnectionString(string? connectionString);
}
