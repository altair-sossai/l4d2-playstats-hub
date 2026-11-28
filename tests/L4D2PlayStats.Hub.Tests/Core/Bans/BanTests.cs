using L4D2PlayStats.Hub.Core.Modules.Bans;
using L4D2PlayStats.Hub.Core.Modules.Bans.Enums;
using L4D2PlayStats.Hub.Core.Modules.Bans.Extensions;
using L4D2PlayStats.Hub.Core.Modules.Bans.Models;
using L4D2PlayStats.Hub.Core.Modules.Bans.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Core.Bans;

[TestClass]
public class BanTests
{
    private const long Alice = 76561198000000001;
    private const long Bob = 76561198000000002;
    private const long Admin = 76561198000000100;
    private static readonly BanActor Administrator = new(Admin, "Admin");

    [TestMethod]
    public async Task ASiteBanRecordsWhoBannedWhyAndWhenItEnds()
    {
        using var hub = new TestHub();

        var endsAt = hub.Clock.UtcNow.AddDays(3);
        var result = await AddAsync(hub, Alice, endsAt, " Cheating ", " Alice ");
        var ban = await ActiveAsync(hub, Alice);

        Assert.AreEqual(BanSaveResult.Saved, result);
        Assert.AreEqual("Alice", ban!.Name);
        Assert.AreEqual("Cheating", ban.Reason);
        Assert.AreEqual(BanSource.Site, ban.Source);
        Assert.AreEqual(Admin, ban.CreatedByCommunityId);
        Assert.AreEqual("Admin", ban.CreatedByName);
        Assert.AreEqual(hub.Clock.UtcNow, ban.CreatedAtUtc);
        Assert.AreEqual(endsAt, ban.EndsAtUtc);
    }

    [TestMethod]
    public async Task APlayerWhoIsBannedCannotBeBannedAgainFromTheSite()
    {
        using var hub = new TestHub();

        await AddAsync(hub, Alice, null, "Cheating");

        var again = await AddAsync(hub, Alice, null, "Insults");
        var invalid = await AddAsync(hub, 12, null, "Cheating");
        var past = await AddAsync(hub, Bob, hub.Clock.UtcNow.AddMinutes(-1), "Cheating");

        Assert.AreEqual(BanSaveResult.AlreadyBanned, again);
        Assert.AreEqual(BanSaveResult.InvalidPlayer, invalid);
        Assert.AreEqual(BanSaveResult.InvalidEnd, past);
        Assert.AreEqual(1, await CountAsync(hub, BanListFilter.Active));
    }

    [TestMethod]
    public async Task ABanEndsByItselfWhenItsTimeIsOver()
    {
        using var hub = new TestHub();

        await AddAsync(hub, Alice, hub.Clock.UtcNow.AddHours(1), "Rage quit");
        await AddAsync(hub, Bob, null, "Cheating");

        hub.Clock.Advance(TimeSpan.FromHours(2));

        var ended = await hub.RunAsync(services => services.GetRequiredService<IBanService>().PageAsync(PlayerSearch.Of(null), BanListFilter.Ended, 1, 20));

        Assert.IsNull(await ActiveAsync(hub, Alice));
        Assert.IsNotNull(await ActiveAsync(hub, Bob));
        Assert.AreEqual(Alice, ended.Items.Single().CommunityId);
        Assert.AreEqual(BanStatus.Expired, ended.Items.Single().Status(hub.Clock.UtcNow));
        Assert.AreEqual(BanSaveResult.Saved, await AddAsync(hub, Alice, null, "Rage quit again"));
    }

    [TestMethod]
    public async Task EditingABanChangesItsEndAndReasonAndLiftingItKeepsTheHistory()
    {
        using var hub = new TestHub();

        await AddAsync(hub, Alice, null, "Cheating");

        var ban = await ActiveAsync(hub, Alice);
        var endsAt = hub.Clock.UtcNow.AddDays(7);

        hub.Clock.Advance(TimeSpan.FromMinutes(5));

        var edited = await hub.RunAsync(services => services.GetRequiredService<IBanService>().EditAsync(ban!.Id, endsAt, "Toxic", new BanActor(Bob, "Bob")));
        var afterEdit = await ActiveAsync(hub, Alice);
        var lifted = await hub.RunAsync(services => services.GetRequiredService<IBanService>().LiftAsync(ban!.Id, " Appeal accepted ", Administrator));
        var liftedAgain = await hub.RunAsync(services => services.GetRequiredService<IBanService>().LiftAsync(ban!.Id, null, Administrator));
        var history = await hub.RunAsync(services => services.GetRequiredService<IBanService>().HistoryAsync(Alice));

        Assert.AreEqual(BanSaveResult.Saved, edited);
        Assert.AreEqual(endsAt, afterEdit!.EndsAtUtc);
        Assert.AreEqual("Toxic", afterEdit.Reason);
        Assert.AreEqual(Bob, afterEdit.UpdatedByCommunityId);
        Assert.AreEqual(hub.Clock.UtcNow, afterEdit.UpdatedAtUtc);
        Assert.IsTrue(lifted);
        Assert.IsFalse(liftedAgain);
        Assert.IsNull(await ActiveAsync(hub, Alice));
        Assert.HasCount(1, history);
        Assert.AreEqual(BanStatus.Lifted, history[0].Status(hub.Clock.UtcNow));
        Assert.AreEqual("Appeal accepted", history[0].LiftReason);
        Assert.AreEqual(Admin, history[0].LiftedByCommunityId);
    }

    [TestMethod]
    public async Task ABanOfTheGameServerIsRecordedWithTheCommandTheAdminAndTheServer()
    {
        using var hub = new TestHub();

        var ban = await CaptureAsync(hub, new GameBanCommand { Player = "STEAM_1:1:19867136", Name = "Alice", Minutes = 60, Reason = "Rage quit", Command = "sm_ban", AdminCommunityId = Admin.ToString(), AdminName = "Admin" });

        Assert.AreEqual(Alice, ban!.CommunityId);
        Assert.AreEqual("Alice", ban.Name);
        Assert.AreEqual("Rage quit", ban.Reason);
        Assert.AreEqual(BanSource.Game, ban.Source);
        Assert.AreEqual("sm_ban", ban.Command);
        Assert.AreEqual("Server", ban.GameServerName);
        Assert.AreEqual(Admin, ban.CreatedByCommunityId);
        Assert.AreEqual(hub.Clock.UtcNow.AddMinutes(60), ban.EndsAtUtc);
        Assert.IsNull(await CaptureAsync(hub, new GameBanCommand { Player = "nobody", Minutes = 0 }));
        Assert.IsNull(await CaptureAsync(hub, new GameBanCommand { Player = Bob.ToString(), Minutes = -1 }));
    }

    [TestMethod]
    public async Task BanningAgainOnTheGameServerUpdatesTheActiveBanAndAnUnbanLiftsIt()
    {
        using var hub = new TestHub();

        await CaptureAsync(hub, new GameBanCommand { Player = Alice.ToString(), Name = "Alice", Minutes = 60, Reason = "Abandoned the mix", Command = "l4d2_mix" });

        hub.Clock.Advance(TimeSpan.FromMinutes(10));

        await CaptureAsync(hub, new GameBanCommand { Player = Alice.ToString(), Minutes = 0, Reason = "Cheating", Command = "sm_addban", AdminCommunityId = Admin.ToString(), AdminName = "Admin" });

        var history = await hub.RunAsync(services => services.GetRequiredService<IBanService>().HistoryAsync(Alice));
        var lifted = await hub.RunAsync(services => services.GetRequiredService<IBanService>().CaptureLiftAsync(new GameUnbanCommand { Player = Alice.ToString(), Command = "sm_unban", AdminName = "Admin" }));

        Assert.HasCount(1, history);
        Assert.AreEqual("Alice", history[0].Name);
        Assert.AreEqual("Cheating", history[0].Reason);
        Assert.AreEqual("sm_addban", history[0].Command);
        Assert.IsNull(history[0].EndsAtUtc);
        Assert.AreEqual(Admin, history[0].UpdatedByCommunityId);
        Assert.IsTrue(lifted);
        Assert.IsNull(await ActiveAsync(hub, Alice));
    }

    [TestMethod]
    public async Task EachAttemptOfABannedPlayerToConnectIsCounted()
    {
        using var hub = new TestHub();

        await AddAsync(hub, Alice, null, "Cheating");

        await hub.RunAsync(services => services.GetRequiredService<IBanService>().AttemptAsync(Alice));
        hub.Clock.Advance(TimeSpan.FromMinutes(1));

        var ban = await hub.RunAsync(services => services.GetRequiredService<IBanService>().AttemptAsync(Alice));
        var notBanned = await hub.RunAsync(services => services.GetRequiredService<IBanService>().AttemptAsync(Bob));

        Assert.AreEqual(2, ban!.Attempts);
        Assert.AreEqual(hub.Clock.UtcNow, ban.LastAttemptAtUtc);
        Assert.IsNull(notBanned);
    }

    [TestMethod]
    public async Task EveryChangeMadeOnTheSiteAsksTheGameServersToCheckThePlayer()
    {
        using var hub = new TestHub();

        var credentials = await hub.CreateServerAsync();

        await AddAsync(hub, Alice, null, "Cheating");

        var queued = await hub.RunAsync(services => services.GetRequiredService<IRemoteCommandService>().DequeueAsync(credentials.Server));

        await CaptureAsync(hub, new GameBanCommand { Player = Bob.ToString(), Minutes = 0, Command = "sm_ban" });

        var afterGameBan = await hub.RunAsync(services => services.GetRequiredService<IRemoteCommandService>().DequeueAsync(credentials.Server));

        Assert.AreEqual($"sm_playstats_ban_sync {Alice}", queued!.FullCommand);
        Assert.IsNull(afterGameBan);
    }

    [TestMethod]
    public async Task ThePlayerIsToldWhenBannedAndWhenTheBanIsLifted()
    {
        using var hub = new TestHub();

        await AddAsync(hub, Alice, null, "Cheating");

        var banned = await TextsAsync(hub, Alice);
        var ban = await ActiveAsync(hub, Alice);

        await hub.RunAsync(services => services.GetRequiredService<IBanService>().LiftAsync(ban!.Id, null, Administrator));

        var lifted = await TextsAsync(hub, Alice);

        Assert.AreEqual("You were banned from the game servers. The bans page shows the reason and when it ends.", banned.Single());
        Assert.AreEqual("Your ban from the game servers was lifted. You can play again.", lifted[0]);
    }

    [TestMethod]
    public async Task TheImportReadsBannedUserCfgAndKeepsWhoIsAlreadyBanned()
    {
        using var hub = new TestHub();

        await AddAsync(hub, Bob, null, "Already here");

        const string file = """
                            // Name: Alice
                            // Profile: https://steamcommunity.com/profiles/76561198000000001
                            // Ban date: 2026-08-24
                            // Expire in: never
                            // Ban reason: Frequent rage quits
                            banid 0 STEAM_0:1:19867136
                            banid 0 STEAM_1:1:19867136

                            // Name: Bob
                            // Ban date: 2026-09-18
                            // Ban reason: Alt account
                            banid 0 STEAM_1:0:19867137

                            banid 0 STEAM_1:0:19867140
                            banid 0 not-a-steam-id
                            exec other.cfg
                            """;

        var import = await hub.RunAsync(services => services.GetRequiredService<IBanService>().ImportAsync(file, Administrator));
        var alice = await ActiveAsync(hub, Alice);
        var nameless = await ActiveAsync(hub, 76561198000000001 + 7);

        Assert.AreEqual(2, import.Added);
        Assert.AreEqual(1, import.Unchanged);
        Assert.HasCount(1, import.Invalid);
        Assert.AreEqual("Alice", alice!.Name);
        Assert.AreEqual("Frequent rage quits", alice.Reason);
        Assert.AreEqual(new DateTime(2026, 8, 24, 0, 0, 0, DateTimeKind.Utc), alice.CreatedAtUtc);
        Assert.IsNull(alice.EndsAtUtc);
        Assert.AreEqual(BanSource.Import, alice.Source);
        Assert.IsNotNull(nameless);
        Assert.IsNull(nameless.Reason);
    }

    [TestMethod]
    public async Task TheDatesOfTheImportAreDaysOfTheTimeZoneOfTheSite()
    {
        using var hub = new TestHub();

        await hub.RunAsync(services => services.GetRequiredService<ISettingsService>().SaveAsync(new SiteSettings { Name = "Hub", TimeZone = "America/Sao_Paulo" }));
        await hub.RunAsync(services => services.GetRequiredService<IBanService>().ImportAsync("// Name: Alice\n// Ban date: 2026-08-24\n// Expire in: 2026-12-25\nbanid 0 STEAM_1:1:19867136", Administrator));

        var alice = await ActiveAsync(hub, Alice);

        Assert.AreEqual(new DateTime(2026, 8, 24, 3, 0, 0, DateTimeKind.Utc), alice!.CreatedAtUtc);
        Assert.AreEqual(new DateTime(2026, 12, 25, 3, 0, 0, DateTimeKind.Utc), alice.EndsAtUtc);
    }

    [TestMethod]
    public async Task TheSearchFindsBansByNameSteamIdOrReason()
    {
        using var hub = new TestHub();

        await AddAsync(hub, Alice, null, "Wallhack", "Alice");
        await AddAsync(hub, Bob, null, "Rage quit", "Bob");

        async Task<List<long>> SearchAsync(string term)
        {
            var page = await hub.RunAsync(services => services.GetRequiredService<IBanService>().PageAsync(PlayerSearch.Of(term), BanListFilter.Active, 1, 20));

            return [.. page.Items.Select(ban => ban.CommunityId)];
        }

        CollectionAssert.AreEqual(new[] { Alice }, await SearchAsync("wallhack"));
        CollectionAssert.AreEqual(new[] { Bob }, await SearchAsync("bob"));
        CollectionAssert.AreEqual(new[] { Bob }, await SearchAsync(Bob.ToString()));
    }

    internal static Task<BanSaveResult> AddAsync(TestHub hub, long communityId, DateTime? endsAt, string? reason, string? name = null)
    {
        return hub.RunAsync(services => services.GetRequiredService<IBanService>().AddAsync(new BanRequest(communityId, name, endsAt, reason), Administrator));
    }

    internal static Task<Ban?> ActiveAsync(TestHub hub, long communityId)
    {
        return hub.RunAsync(services => services.GetRequiredService<IBanService>().ActiveAsync(communityId));
    }

    internal static Task<Ban?> CaptureAsync(TestHub hub, GameBanCommand command)
    {
        return hub.RunAsync(services => services.GetRequiredService<IBanService>().CaptureAsync(command, "Server"));
    }

    internal static Task EnableAsync(TestHub hub)
    {
        return hub.RunAsync(services => services.GetRequiredService<ISettingsService>().SaveAsync(new ModuleSettings { Ranking = true, LiveServer = true, Bans = true }));
    }

    private static Task<int> CountAsync(TestHub hub, BanListFilter filter)
    {
        return hub.RunAsync(services => services.GetRequiredService<IBanService>().CountAsync(filter));
    }

    private static async Task<List<string>> TextsAsync(TestHub hub, long communityId)
    {
        var items = await hub.RunAsync(services => services.GetRequiredService<INotificationService>().LatestAsync(communityId, false, 10));

        return [.. items.Select(item => item.Text)];
    }
}
