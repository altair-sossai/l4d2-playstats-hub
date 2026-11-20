using L4D2PlayStats.Hub.Core.Modules.Backups.Settings;
using L4D2PlayStats.Hub.Web.Infrastructure.Backups;
using L4D2PlayStats.Hub.Web.Infrastructure.Backups.Contracts;

namespace L4D2PlayStats.Hub.Tests.Support.Backups;

public class FakeBackupStorageFactory : IBackupStorageFactory
{
    private readonly AzureBackupStorageFactory _azure = new();

    public FakeBackupStorage Storage { get; } = new();
    public List<BackupSettings> Created { get; } = [];

    public IBackupStorage Create(BackupSettings settings)
    {
        Created.Add(settings);

        return Storage;
    }

    public bool IsValidConnectionString(string? connectionString)
    {
        return _azure.IsValidConnectionString(connectionString);
    }
}
