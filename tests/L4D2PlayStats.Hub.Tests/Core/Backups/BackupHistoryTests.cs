using L4D2PlayStats.Hub.Core.Modules.Backups.Enums;
using L4D2PlayStats.Hub.Core.Modules.Backups.Models;
using L4D2PlayStats.Hub.Core.Modules.Backups.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Core.Backups;

[TestClass]
public class BackupHistoryTests
{
    [TestMethod]
    public async Task TheHistoryStartsEmpty()
    {
        using var hub = new TestHub();

        var history = await hub.RunAsync(services => services.GetRequiredService<IBackupHistoryService>().GetAsync());

        Assert.IsEmpty(history.Runs);
    }

    [TestMethod]
    public async Task TheHistoryKeepsTheLastTwentyRunsNewestFirst()
    {
        using var hub = new TestHub();

        var start = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);

        for (var index = 0; index < 25; index++)
            await hub.RunAsync(services => services.GetRequiredService<IBackupHistoryService>().AddAsync(new BackupRun
            {
                StartedAtUtc = start.AddHours(8 * index),
                Succeeded = index % 2 == 0,
                Error = index % 2 == 0 ? null : "failed",
                BlobName = $"hub-backup-{index}.zip",
                SizeInBytes = index,
                Trigger = BackupTrigger.Schedule
            }));

        var history = await hub.RunAsync(services => services.GetRequiredService<IBackupHistoryService>().GetAsync());

        Assert.HasCount(20, history.Runs);
        Assert.AreEqual("hub-backup-24.zip", history.Runs[0].BlobName);
        Assert.AreEqual("hub-backup-5.zip", history.Runs[^1].BlobName);
        Assert.IsTrue(history.Runs[0].Succeeded);
        Assert.AreEqual("failed", history.Runs[1].Error);
    }
}