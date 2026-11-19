using L4D2PlayStats.Hub.Core.Modules.Backups.Helpers;

namespace L4D2PlayStats.Hub.Tests.Core.Backups;

[TestClass]
public class BackupNamesTests
{
    [TestMethod]
    public void TheFileIsNamedAfterThePrefixAndTheUtcTime()
    {
        var name = BackupNames.FileName("hub-backup", new DateTime(2026, 11, 19, 3, 4, 5, DateTimeKind.Utc));

        Assert.AreEqual("hub-backup-20261119-030405.zip", name);
        Assert.IsTrue(BackupNames.IsBackup("hub-backup", name));
    }

    [TestMethod]
    [DataRow("hub-backup-20261119-030405.zip", true)]
    [DataRow("hub-backup-20261119-030405.txt", false)]
    [DataRow("hub-backup-connection-test-0123.txt", false)]
    [DataRow("hub-backup-extra-20261119-030405.zip", false)]
    [DataRow("other-20261119-030405.zip", false)]
    [DataRow("hub-backup-2026111-030405.zip", false)]
    public void OnlyFilesWithThePrefixAndTheDateAreBackups(string name, bool expected)
    {
        Assert.AreEqual(expected, BackupNames.IsBackup("hub-backup", name));
    }

    [TestMethod]
    public void APrefixWithDotsIsMatchedLiterally()
    {
        Assert.IsTrue(BackupNames.IsBackup("l4d2.com.br", "l4d2.com.br-20261119-030405.zip"));
        Assert.IsFalse(BackupNames.IsBackup("l4d2.com.br", "l4d2xcomxbr-20261119-030405.zip"));
    }

    [TestMethod]
    public void TheOldestBackupsBeyondTheNumberToKeepExpire()
    {
        var names = new[]
        {
            "hub-backup-20261117-000000.zip",
            "hub-backup-20261119-080000.zip",
            "hub-backup-20261118-160000.zip",
            "hub-backup-20261119-000000.zip"
        };

        var expired = BackupNames.Expired("hub-backup", names, 2);

        CollectionAssert.AreEqual(new[] { "hub-backup-20261118-160000.zip", "hub-backup-20261117-000000.zip" }, expired);
    }

    [TestMethod]
    public void FilesThatAreNotBackupsOfThisPrefixNeverExpire()
    {
        var names = new[]
        {
            "hub-backup-20261117-000000.zip",
            "hub-backup-20261118-000000.zip",
            "hub-backup-connection-test-abc.txt",
            "other-20261101-000000.zip",
            "notes.txt"
        };

        var expired = BackupNames.Expired("hub-backup", names, 1);

        CollectionAssert.AreEqual(new[] { "hub-backup-20261117-000000.zip" }, expired);
    }

    [TestMethod]
    public void AtLeastTheNewestBackupIsAlwaysKept()
    {
        var names = new[] { "hub-backup-20261117-000000.zip", "hub-backup-20261118-000000.zip" };

        CollectionAssert.AreEqual(new[] { "hub-backup-20261117-000000.zip" }, BackupNames.Expired("hub-backup", names, 0));
    }

    [TestMethod]
    public void NothingExpiresWhileThereAreFewerBackupsThanTheNumberToKeep()
    {
        var names = new[] { "hub-backup-20261117-000000.zip", "hub-backup-20261118-000000.zip" };

        Assert.IsEmpty(BackupNames.Expired("hub-backup", names, 30));
    }
}
