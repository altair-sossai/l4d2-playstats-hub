using L4D2PlayStats.Hub.Core.Modules.Backups.Helpers;

namespace L4D2PlayStats.Hub.Tests.Core.Backups;

[TestClass]
public class BackupRulesTests
{
    [TestMethod]
    [DataRow("https://account.blob.core.windows.net/backups?sv=2024-11-04&sp=racwdl&sig=abc", true)]
    [DataRow(" https://account.blob.core.windows.net/backups?sig=abc ", true)]
    [DataRow("http://account.blob.core.windows.net/backups?sig=abc", false)]
    [DataRow("https://account.blob.core.windows.net/backups", false)]
    [DataRow("https://account.blob.core.windows.net/?sig=abc", false)]
    [DataRow("account.blob.core.windows.net/backups?sig=abc", false)]
    [DataRow("", false)]
    [DataRow(null, false)]
    public void AContainerAddressNeedsHttpsAContainerAndASignature(string? address, bool expected)
    {
        Assert.AreEqual(expected, BackupRules.IsValidContainerUrl(address));
    }

    [TestMethod]
    [DataRow("backups", true)]
    [DataRow("hub-backups-2026", true)]
    [DataRow("abc", true)]
    [DataRow("ab", false)]
    [DataRow("Backups", false)]
    [DataRow("hub--backups", false)]
    [DataRow("-backups", false)]
    [DataRow("backups-", false)]
    [DataRow("hub_backups", false)]
    [DataRow(null, false)]
    public void AContainerNameFollowsTheAzureRules(string? name, bool expected)
    {
        Assert.AreEqual(expected, BackupRules.IsValidContainerName(name));
    }

    [TestMethod]
    [DataRow("hub-backup", true)]
    [DataRow("l4d2.com.br", true)]
    [DataRow("hub_backup_2", true)]
    [DataRow("-hub", false)]
    [DataRow("hub backup", false)]
    [DataRow("hub/backup", false)]
    [DataRow("", false)]
    [DataRow(null, false)]
    public void APrefixIsMadeOfSafeCharacters(string? prefix, bool expected)
    {
        Assert.AreEqual(expected, BackupRules.IsValidPrefix(prefix));
    }

    [TestMethod]
    public void APrefixHasAtMostSixtyCharacters()
    {
        Assert.IsTrue(BackupRules.IsValidPrefix(new string('a', 60)));
        Assert.IsFalse(BackupRules.IsValidPrefix(new string('a', 61)));
    }

    [TestMethod]
    [DataRow(-5, 1)]
    [DataRow(0, 1)]
    [DataRow(8, 8)]
    [DataRow(168, 168)]
    [DataRow(500, 168)]
    public void TheIntervalStaysBetweenOneHourAndOneWeek(int hours, int expected)
    {
        Assert.AreEqual(expected, BackupRules.Interval(hours));
    }

    [TestMethod]
    [DataRow(0, 1)]
    [DataRow(30, 30)]
    [DataRow(5000, 1000)]
    public void TheNumberToKeepStaysWithinItsLimits(int count, int expected)
    {
        Assert.AreEqual(expected, BackupRules.Keep(count));
    }
}
