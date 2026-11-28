using System.IO.Compression;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Backups.Enums;
using L4D2PlayStats.Hub.Core.Modules.Backups.Models;
using L4D2PlayStats.Hub.Core.Modules.Backups.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Backups.Settings;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Enums;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web.Backups;

[TestClass]
public class BackupServiceTests
{
    private const string Signature = "sv=2024-11-04&sp=racwdl&sig=c2VjcmV0LXNpZw%3D%3D";
    private const string ContainerUrl = "https://account.blob.core.windows.net/backups?" + Signature;

    [TestMethod]
    public async Task ABackupUploadsOneZipNamedAfterThePrefixAndTheTime()
    {
        using var factory = await StartAsync();

        var run = await RunAsync(factory, BackupTrigger.Manual);
        var storage = factory.Backups.Storage;

        Assert.IsNotNull(run);
        Assert.IsTrue(run.Succeeded, run.Error);
        Assert.HasCount(1, storage.Blobs);
        Assert.AreEqual(run.BlobName, storage.Blobs.Keys.Single());
        Assert.MatchesRegex(@"^hub-backup-\d{8}-\d{6}\.zip$", run.BlobName!);
        Assert.AreEqual("application/zip", storage.ContentTypes[run.BlobName!]);
        Assert.AreEqual(storage.Blobs[run.BlobName!].LongLength, run.SizeInBytes);
        Assert.AreEqual(BackupTrigger.Manual, run.Trigger);
        Assert.AreEqual(1, storage.Prepared);

        using var zip = new ZipArchive(new MemoryStream(storage.Blobs[run.BlobName!]));

        Assert.IsNotNull(zip.GetEntry("manifest.json"));
    }

    [TestMethod]
    public async Task EachRunIsRecordedInTheHistory()
    {
        using var factory = await StartAsync();

        await RunAsync(factory, BackupTrigger.Manual);

        var history = await HistoryAsync(factory);

        Assert.HasCount(1, history.Runs);
        Assert.IsTrue(history.Runs[0].Succeeded);
        Assert.IsGreaterThan(0L, history.Runs[0].SizeInBytes);
        Assert.IsGreaterThanOrEqualTo(0L, history.Runs[0].DurationInMilliseconds);
    }

    [TestMethod]
    public async Task TheOldestBackupsBeyondTheNumberToKeepAreDeleted()
    {
        using var factory = await StartAsync(settings => settings.Keep = 2);

        var storage = factory.Backups.Storage;

        storage.Blobs["hub-backup-20261101-000000.zip"] = [1];
        storage.Blobs["hub-backup-20261102-000000.zip"] = [2];
        storage.Blobs["hub-backup-20261103-000000.zip"] = [3];
        storage.Blobs["hub-backup-notes.txt"] = [4];

        var run = await RunAsync(factory, BackupTrigger.Schedule);

        CollectionAssert.AreEquivalent(new[] { run!.BlobName, "hub-backup-20261103-000000.zip", "hub-backup-notes.txt" }, storage.Blobs.Keys.ToList());
        CollectionAssert.AreEquivalent(new[] { "hub-backup-20261101-000000.zip", "hub-backup-20261102-000000.zip" }, storage.Deleted);
    }

    [TestMethod]
    public async Task OnlyTheBackupsOfTheSamePrefixAreDeleted()
    {
        using var factory = await StartAsync(settings =>
        {
            settings.Keep = 1;
            settings.Prefix = "l4d2.com.br";
        });

        var storage = factory.Backups.Storage;

        storage.Blobs["hub-backup-20261101-000000.zip"] = [1];
        storage.Blobs["l4d2.com.br-20261101-000000.zip"] = [2];

        var run = await RunAsync(factory, BackupTrigger.Schedule);

        StringAssert.StartsWith(run!.BlobName, "l4d2.com.br-");
        CollectionAssert.AreEquivalent(new[] { run.BlobName, "hub-backup-20261101-000000.zip" }, storage.Blobs.Keys.ToList());
    }

    [TestMethod]
    public async Task AFailedUploadIsRecordedWithoutTheSecret()
    {
        using var factory = await StartAsync();

        factory.Backups.Storage.FailUploadWith = $"Server failed to authenticate the request for {ContainerUrl}.\nStatus: 403\nHeaders: x";

        var run = await RunAsync(factory, BackupTrigger.Schedule);
        var history = await HistoryAsync(factory);

        Assert.IsFalse(run!.Succeeded);
        Assert.IsEmpty(factory.Backups.Storage.Blobs);
        Assert.AreEqual("Server failed to authenticate the request for ••••••••.", run.Error);
        Assert.HasCount(1, history.Runs);
        Assert.IsFalse(history.Runs[0].Succeeded);
        Assert.AreEqual(run.Error, history.Runs[0].Error);
    }

    [TestMethod]
    public async Task ABackupWithoutADestinationFailsAndUploadsNothing()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        var run = await RunAsync(factory, BackupTrigger.Manual);

        Assert.IsFalse(run!.Succeeded);
        Assert.AreEqual("The backup destination is not set.", run.Error);
        Assert.IsEmpty(factory.Backups.Created);
    }

    [TestMethod]
    public async Task AFailureToDeleteOldBackupsDoesNotFailTheBackup()
    {
        using var factory = await StartAsync();

        factory.Backups.Storage.FailListWith = "The list failed.";

        var run = await RunAsync(factory, BackupTrigger.Schedule);

        Assert.IsTrue(run!.Succeeded);
        Assert.HasCount(1, factory.Backups.Storage.Blobs);
    }

    [TestMethod]
    public async Task OnlyOneBackupRunsAtATime()
    {
        using var factory = await StartAsync();

        BackupRun? second = null;
        var running = false;

        factory.Backups.Storage.DuringUpload = async () =>
        {
            using var scope = factory.Services.CreateScope();

            var service = scope.ServiceProvider.GetRequiredService<IBackupService>();

            running = service.IsRunning;
            second = await service.RunAsync(BackupTrigger.Manual);
        };

        var first = await RunAsync(factory, BackupTrigger.Manual);

        Assert.IsTrue(first!.Succeeded);
        Assert.IsTrue(running);
        Assert.IsNull(second);
        Assert.HasCount(1, factory.Backups.Storage.Blobs);
        Assert.IsFalse(await factory.RunAsync(services => Task.FromResult(services.GetRequiredService<IBackupService>().IsRunning)));
    }

    [TestMethod]
    public async Task TheScheduleRunsABackupOnlyWhenOneIsDue()
    {
        using var factory = await StartAsync();

        var first = await factory.RunAsync(services => services.GetRequiredService<IBackupService>().RunIfDueAsync());
        var second = await factory.RunAsync(services => services.GetRequiredService<IBackupService>().RunIfDueAsync());

        Assert.IsNotNull(first);
        Assert.AreEqual(BackupTrigger.Schedule, first.Trigger);
        Assert.IsNull(second);
        Assert.HasCount(1, factory.Backups.Storage.Blobs);
    }

    [TestMethod]
    public async Task TheScheduleRunsAgainOnceTheIntervalHasPassed()
    {
        using var factory = await StartAsync();

        await factory.RunAsync(services => services.GetRequiredService<IBackupHistoryService>().AddAsync(new BackupRun
        {
            StartedAtUtc = DateTime.UtcNow.AddHours(-9),
            Succeeded = true,
            BlobName = "hub-backup-old.zip"
        }));

        var run = await factory.RunAsync(services => services.GetRequiredService<IBackupService>().RunIfDueAsync());

        Assert.IsNotNull(run);
        Assert.IsTrue(run.Succeeded);
    }

    [TestMethod]
    public async Task TheScheduleDoesNothingWhileTheBackupIsOff()
    {
        using var factory = await StartAsync(settings => settings.Enabled = false);

        var run = await factory.RunAsync(services => services.GetRequiredService<IBackupService>().RunIfDueAsync());

        Assert.IsNull(run);
        Assert.IsEmpty(factory.Backups.Created);
        Assert.IsEmpty((await HistoryAsync(factory)).Runs);
    }

    [TestMethod]
    public async Task AManualBackupRunsEvenWhenTheScheduleIsOff()
    {
        using var factory = await StartAsync(settings => settings.Enabled = false);

        var run = await RunAsync(factory, BackupTrigger.Manual);

        Assert.IsTrue(run!.Succeeded);
    }

    [TestMethod]
    public async Task TheConnectionTestWritesListsAndDeletesATestFile()
    {
        using var factory = await StartAsync();

        var check = await factory.RunAsync(services => services.GetRequiredService<IBackupService>().TestAsync());
        var storage = factory.Backups.Storage;

        Assert.IsTrue(check.Succeeded, check.Error);
        Assert.IsEmpty(storage.Blobs);
        Assert.HasCount(1, storage.Deleted);
        StringAssert.StartsWith(storage.Deleted[0], "hub-backup-connection-test-");
        Assert.IsEmpty((await HistoryAsync(factory)).Runs);
    }

    [TestMethod]
    public async Task TheConnectionTestFailsWhenTheTestFileIsNotListed()
    {
        using var factory = await StartAsync();

        factory.Backups.Storage.HideFromList = true;

        var check = await factory.RunAsync(services => services.GetRequiredService<IBackupService>().TestAsync());

        Assert.IsFalse(check.Succeeded);
        Assert.AreEqual("The test failed.", check.Error);
    }

    [TestMethod]
    public async Task TheConnectionTestReportsTheErrorWithoutTheSecret()
    {
        using var factory = await StartAsync();

        factory.Backups.Storage.FailUploadWith = $"This request is not authorized: {Signature}";

        var check = await factory.RunAsync(services => services.GetRequiredService<IBackupService>().TestAsync());

        Assert.IsFalse(check.Succeeded);
        Assert.AreEqual("This request is not authorized: ••••••••", check.Error);
    }

    [TestMethod]
    public async Task TheConnectionTestNeedsADestination()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        var check = await factory.RunAsync(services => services.GetRequiredService<IBackupService>().TestAsync());

        Assert.IsFalse(check.Succeeded);
        Assert.AreEqual("The backup destination is not set.", check.Error);
    }

    [TestMethod]
    public async Task TheStorageIsCreatedFromTheSavedSettings()
    {
        using var factory = await StartAsync(settings =>
        {
            settings.Destination = BackupDestination.ConnectionString;
            settings.ConnectionString = "DefaultEndpointsProtocol=https;AccountName=account;AccountKey=a2V5;EndpointSuffix=core.windows.net";
            settings.ContainerName = "backups";
        });

        await RunAsync(factory, BackupTrigger.Manual);

        Assert.HasCount(1, factory.Backups.Created);
        Assert.AreEqual(BackupDestination.ConnectionString, factory.Backups.Created[0].Destination);
        Assert.AreEqual("backups", factory.Backups.Created[0].ContainerName);
    }

    [TestMethod]
    public async Task OnlyAFailedAutomaticBackupTellsTheAdministrators()
    {
        using var factory = await StartAsync();

        factory.Backups.Storage.FailUploadWith = "The container is gone.";

        await RunAsync(factory, BackupTrigger.Manual);
        await RunAsync(factory, BackupTrigger.Schedule);

        factory.Backups.Storage.FailUploadWith = null;

        await RunAsync(factory, BackupTrigger.Schedule);

        var notices = await factory.RunAsync(services => services.GetRequiredService<INotificationService>().LatestAsync(HubWebFactory.Administrator, true, 10));
        var failures = notices.Where(notice => notice.Topic == NotificationTopic.Backups).ToList();
        var player = await factory.RunAsync(services => services.GetRequiredService<INotificationService>().LatestAsync(HubWebFactory.Player, false, 10));

        Assert.HasCount(1, failures);
        Assert.AreEqual("The automatic backup failed. Open the backup page to see why.", failures[0].Text);
        Assert.AreEqual("/admin/backup", failures[0].Url);
        Assert.IsEmpty(player);

        factory.Backups.Storage.FailDownloadWith = "The blob cannot be read.";

        var run = await RunAsync(factory, BackupTrigger.Schedule);
        var latest = (await factory.RunAsync(services => services.GetRequiredService<INotificationService>().LatestAsync(HubWebFactory.Administrator, true, 10))).First();
        var isSqlite = await factory.RunAsync(services => Task.FromResult(services.GetRequiredService<HubDbContext>().Database.IsSqlite()));

        Assert.IsTrue(run!.Succeeded);
        Assert.AreEqual(isSqlite ? BackupVerificationStatus.Failed : BackupVerificationStatus.NotAvailable, run.Verification!.Status);
        Assert.AreEqual(isSqlite ? "The automatic backup was uploaded, but its check failed. Open the backup page to see why." : "The automatic backup failed. Open the backup page to see why.", latest.Text);
    }

    private static async Task<HubWebFactory> StartAsync(Action<BackupSettings>? change = null)
    {
        var factory = new HubWebFactory();
        var settings = new BackupSettings { Enabled = true, ContainerUrl = ContainerUrl };

        change?.Invoke(settings);

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(settings);

        return factory;
    }

    private static Task<BackupRun?> RunAsync(HubWebFactory factory, BackupTrigger trigger)
    {
        return factory.RunAsync(services => services.GetRequiredService<IBackupService>().RunAsync(trigger));
    }

    private static Task<BackupHistory> HistoryAsync(HubWebFactory factory)
    {
        return factory.RunAsync(services => services.GetRequiredService<IBackupHistoryService>().GetAsync());
    }
}
