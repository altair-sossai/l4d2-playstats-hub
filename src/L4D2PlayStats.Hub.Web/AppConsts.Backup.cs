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

    public const string AdminBackupVerifyUrl = "/admin/backup/verify";
    public const string BackupVerifyFolderPrefix = "verify-";
    public const string BackupVerifyZipName = "backup.zip";
    public const int BackupVerifyBufferSize = 81920;
    public const string BackupCountSqlFormat = "SELECT COUNT(*) FROM \"{0}\"";
    public const string BackupIntegrityCheckSql = "PRAGMA integrity_check";
    public const string BackupIntegrityOk = "ok";

    public const string BackupVerifyStatusTempData = "BackupVerifyStatus";
    public const string BackupVerifyMessageTempData = "BackupVerifyMessage";
    public const string BackupVerifyArgumentsTempData = "BackupVerifyArguments";
    public const string BackupVerifyBlobTempData = "BackupVerifyBlob";

    public const string BackupVerifyPassedMessage = "The zip opened, the database passed the integrity check and has {0} matches, {1} statistics files, {2} players and {3} settings, the same as the site had when the backup was made.";
    public const string BackupVerifyPassedWithoutCountsMessage = "The zip opened and the database passed the integrity check. This backup was made before the row counts were saved, so they were not compared.";
    public const string BackupVerifyNotAvailableMessage = "Not available: this site does not use SQLite, so the backup has no database to verify.";
    public const string BackupVerifyNoManifestMessage = "The zip has no readable manifest.json.";
    public const string BackupVerifyNoDatabaseMessage = "The zip has no hub.db.";
    public const string BackupVerifyMissingFileMessage = "The file {0} is listed in the manifest but is not in the zip.";
    public const string BackupVerifySizeMessage = "The downloaded file has {0} bytes, but the backup uploaded {1}.";
    public const string BackupVerifyIntegrityMessage = "The database failed the integrity check: {0}";
    public const string BackupVerifyCountMessage = "The table {0} has {1} rows in the backup, but the site had {2} when the backup was made.";
    public const string BackupVerifyUnreadableMessage = "The backup could not be read: {0}";
    public const string BackupVerifyNoBackupMessage = "No backup file was found in the destination.";
    public const string BackupVerifyPassedHeadline = "The backup was verified: {0}.";
    public const string BackupVerifyFailedHeadline = "The verification failed.";
    public const string BackupVerifyNotAvailableHeadline = "The verification is not available.";
}
