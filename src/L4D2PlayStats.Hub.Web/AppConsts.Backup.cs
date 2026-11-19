namespace L4D2PlayStats.Hub.Web;

public static partial class AppConsts
{
    public const string AdminBackupUrl = "/admin/backup";
    public const string AdminBackupRunUrl = "/admin/backup/run";
    public const string AdminBackupTestUrl = "/admin/backup/test";

    public const int BackupCheckIntervalInMinutes = 1;
    public const string BackupDatabaseEntry = "hub.db";
    public const string BackupManifestEntry = "manifest.json";
    public const string BackupTempFolder = "l4d2-playstats-hub-backup";
    public const string BackupContentType = "application/zip";
    public const string BackupTestBlobFormat = "{0}-connection-test-{1}.txt";
    public const string BackupTestContent = "L4D2 PlayStats Hub connection test";
    public const string BackupTestContentType = "text/plain";
    public const int BackupErrorMaxLength = 500;
    public const char BackupEntrySeparator = '/';
    public const string BackupUnknownVersion = "unknown";

    public const string BackupMessageTempData = "BackupMessage";
    public const string BackupErrorTempData = "BackupError";
    public const string BackupDetailTempData = "BackupDetail";

    public const string BackupNotConfiguredMessage = "The backup destination is not set.";
    public const string BackupBusyMessage = "A backup is already running. Try again in a moment.";
    public const string BackupDoneMessage = "Backup done: {0}.";
    public const string BackupFailedMessage = "The backup failed.";
    public const string BackupTestPassedMessage = "The destination accepted a test file, listed it and deleted it.";
    public const string BackupTestFailedMessage = "The test failed.";
    public const string BackupContainerUrlIgnoredMessage = "The container address was not changed: it must start with https://, name the container and carry a SAS token (sig=...).";
    public const string BackupConnectionStringIgnoredMessage = "The connection string was not changed: copy it as the Azure portal shows it, under Access keys of the storage account.";
    public const string BackupContainerNameIgnoredMessage = "The container name was not changed: use 3 to 63 lowercase letters, digits and single hyphens.";
    public const string BackupPrefixIgnoredMessage = "The file name prefix was not changed: use up to 60 letters, digits, dots, hyphens and underscores, starting with a letter or a digit.";
    public const string BackupValidationContainer = "validation";
}
