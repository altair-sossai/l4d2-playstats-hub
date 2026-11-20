using L4D2PlayStats.Hub.Core.Modules.Backups.Enums;
using L4D2PlayStats.Hub.Core.Modules.Backups.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Backups.Models;
using L4D2PlayStats.Hub.Core.Modules.Backups.Settings;

namespace L4D2PlayStats.Hub.Tests.Core.Backups;

[TestClass]
public class BackupScheduleTests
{
    private const string ContainerUrl = "https://account.blob.core.windows.net/backups?sv=2024-11-04&sp=racwdl&sig=secret";
    private static readonly DateTime Now = new(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc);

    [TestMethod]
    public void AnEnabledAndConfiguredBackupThatNeverRanIsDueRightAway()
    {
        var history = new BackupHistory();

        Assert.IsTrue(BackupSchedule.IsDue(Settings(), history, Now));
        Assert.AreEqual(DateTime.MinValue, BackupSchedule.NextAtUtc(Settings(), history));
    }

    [TestMethod]
    public void ABackupIsDueWhenTheIntervalHasPassedSinceTheLastSuccess()
    {
        var history = History(Run(Now.AddHours(-8), true));

        Assert.IsTrue(BackupSchedule.IsDue(Settings(), history, Now));
        Assert.AreEqual(Now, BackupSchedule.NextAtUtc(Settings(), history));
    }

    [TestMethod]
    public void ABackupIsNotDueBeforeTheIntervalHasPassed()
    {
        var history = History(Run(Now.AddHours(-7).AddMinutes(-59), true));

        Assert.IsFalse(BackupSchedule.IsDue(Settings(), history, Now));
        Assert.AreEqual(Now.AddMinutes(1), BackupSchedule.NextAtUtc(Settings(), history));
    }

    [TestMethod]
    public void TheIntervalComesFromTheSettings()
    {
        var settings = Settings();
        var history = History(Run(Now.AddHours(-2), true));

        settings.IntervalInHours = 1;

        Assert.IsTrue(BackupSchedule.IsDue(settings, history, Now));

        settings.IntervalInHours = 3;

        Assert.IsFalse(BackupSchedule.IsDue(settings, history, Now));
    }

    [TestMethod]
    public void AnIntervalOutOfRangeIsKeptWithinTheLimits()
    {
        var settings = Settings();
        var history = History(Run(Now.AddHours(-1), true));

        settings.IntervalInHours = 0;

        Assert.IsTrue(BackupSchedule.IsDue(settings, history, Now));

        settings.IntervalInHours = 10000;

        Assert.AreEqual(Now.AddHours(167), BackupSchedule.NextAtUtc(settings, history));
    }

    [TestMethod]
    public void ADisabledBackupIsNeverDue()
    {
        var settings = Settings();

        settings.Enabled = false;

        Assert.IsFalse(BackupSchedule.IsDue(settings, new BackupHistory(), Now));
        Assert.IsNull(BackupSchedule.NextAtUtc(settings, new BackupHistory()));
    }

    [TestMethod]
    public void ABackupWithoutADestinationIsNeverDue()
    {
        var settings = Settings();

        settings.ContainerUrl = null;

        Assert.IsFalse(BackupSchedule.IsDue(settings, new BackupHistory(), Now));
        Assert.IsNull(BackupSchedule.NextAtUtc(settings, new BackupHistory()));
    }

    [TestMethod]
    public void AfterAFailureTheNextTryWaitsHalfAnHour()
    {
        var history = History(Run(Now.AddHours(-20), true), Run(Now.AddMinutes(-10), false));

        Assert.IsFalse(BackupSchedule.IsDue(Settings(), history, Now));
        Assert.AreEqual(Now.AddMinutes(20), BackupSchedule.NextAtUtc(Settings(), history));
        Assert.IsTrue(BackupSchedule.IsDue(Settings(), history, Now.AddMinutes(20)));
    }

    [TestMethod]
    public void AFailureBeforeAnyBackupAlsoWaitsHalfAnHour()
    {
        var history = History(Run(Now.AddMinutes(-29), false));

        Assert.IsFalse(BackupSchedule.IsDue(Settings(), history, Now));
        Assert.IsTrue(BackupSchedule.IsDue(Settings(), history, Now.AddMinutes(1)));
    }

    [TestMethod]
    public void AFailureDoesNotBringTheNextBackupForward()
    {
        var history = History(Run(Now.AddHours(-1), true), Run(Now.AddMinutes(-40), false));

        Assert.AreEqual(Now.AddHours(7), BackupSchedule.NextAtUtc(Settings(), history));
    }

    [TestMethod]
    public void AnOlderFailureDoesNotDelayAfterALaterSuccess()
    {
        var history = History(Run(Now.AddHours(-9), false), Run(Now.AddHours(-8), true));

        Assert.IsTrue(BackupSchedule.IsDue(Settings(), history, Now));
    }

    [TestMethod]
    public void AManualBackupCountsAsTheLastSuccess()
    {
        var manual = Run(Now.AddHours(-1), true);

        manual.Trigger = BackupTrigger.Manual;

        Assert.IsFalse(BackupSchedule.IsDue(Settings(), History(Run(Now.AddHours(-10), true), manual), Now));
    }

    [TestMethod]
    public void AConnectionStringDestinationNeedsAValidContainerName()
    {
        var settings = Settings();

        settings.Destination = BackupDestination.ConnectionString;
        settings.ConnectionString = "DefaultEndpointsProtocol=https;AccountName=account;AccountKey=a2V5;EndpointSuffix=core.windows.net";

        Assert.IsFalse(BackupSchedule.IsDue(settings, new BackupHistory(), Now));

        settings.ContainerName = "backups";

        Assert.IsTrue(BackupSchedule.IsDue(settings, new BackupHistory(), Now));
    }

    private static BackupSettings Settings()
    {
        return new BackupSettings { Enabled = true, ContainerUrl = ContainerUrl };
    }

    private static BackupHistory History(params BackupRun[] runs)
    {
        return new BackupHistory { Runs = [.. runs] };
    }

    private static BackupRun Run(DateTime startedAtUtc, bool succeeded)
    {
        return new BackupRun { StartedAtUtc = startedAtUtc, Succeeded = succeeded, Trigger = BackupTrigger.Schedule };
    }
}
