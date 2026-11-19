using Azure.Storage.Blobs;
using L4D2PlayStats.Hub.Core.Modules.Backups.Enums;
using L4D2PlayStats.Hub.Core.Modules.Backups.Settings;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Backups;

public class AzureBackupStorageFactory : IBackupStorageFactory
{
    public IBackupStorage Create(BackupSettings settings)
    {
        if (settings.Destination == BackupDestination.ContainerUrl)
            return new AzureBlobBackupStorage(new BlobContainerClient(new Uri(settings.ContainerUrl!.Trim())), false);

        return new AzureBlobBackupStorage(new BlobContainerClient(settings.ConnectionString!.Trim(), settings.ContainerName!.Trim()), true);
    }

    public bool IsValidConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return false;

        try
        {
            _ = new BlobContainerClient(connectionString.Trim(), AppConsts.BackupValidationContainer);

            return true;
        }
        catch (Exception exception) when (exception is FormatException or ArgumentException or InvalidOperationException)
        {
            return false;
        }
    }
}
