using L4D2PlayStats.Hub.Core.Modules.Bans.Models;
using L4D2PlayStats.Hub.Core.Modules.Bans.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;
using static L4D2PlayStats.Hub.Tests.Core.Bans.BanTests;

namespace L4D2PlayStats.Hub.Tests.Core.Bans;

[TestClass]
public class BanAlertTests
{
    private const long Banned = 76561198000000011;
    private const long Alternate = 76561198000000012;
    private const long Other = 76561198000000013;
    private const long Admin = 76561198000000100;
    private const string Home = "10.0.0.1";
    private const string Elsewhere = "10.0.0.2";

    [TestMethod]
    public async Task AnAccountThatConnectsFromTheAddressOfABannedAccountAfterTheBanRaisesAnAlert()
    {
        using var hub = new TestHub();

        await EnableAsync(hub);
        await ConnectAsync(hub, Banned, Home);
        await AddAsync(hub, Banned, null, "Wallhack", "Banned");
        hub.Clock.Advance(TimeSpan.FromHours(1));

        var created = await ConnectAsync(hub, Alternate, Home);
        var again = await ConnectAsync(hub, Alternate, Home);
        var alerts = await AlertsAsync(hub);
        var told = await hub.RunAsync(services => services.GetRequiredService<INotificationService>().LatestAsync(Admin, true, 10));

        Assert.AreEqual(1, created);
        Assert.AreEqual(0, again);
        Assert.AreEqual(Alternate, alerts.Single().Alert.CommunityId);
        Assert.AreEqual(Banned, alerts.Single().Ban.CommunityId);
        Assert.AreEqual("{0} shares an IP address with {1}, who is banned from the game servers.", told.First().Text);
        CollectionAssert.AreEqual(new[] { Alternate.ToString(), Banned.ToString() }, told.First().Values.ToArray());
    }

    [TestMethod]
    public async Task AnAccountThatWasAlreadyLinkedWhenTheSiteBanWasGivenRaisesNoAlert()
    {
        using var hub = new TestHub();

        await EnableAsync(hub);
        await ConnectAsync(hub, Banned, Home);
        await ConnectAsync(hub, Alternate, Home);
        await AddAsync(hub, Banned, null, "Wallhack");
        hub.Clock.Advance(TimeSpan.FromHours(1));

        Assert.AreEqual(0, await ConnectAsync(hub, Alternate, Home));
        Assert.IsEmpty(await AlertsAsync(hub));
    }

    [TestMethod]
    public async Task TheBannedPlayerConnectingFromANewAddressRaisesAnAlertForTheAccountsThere()
    {
        using var hub = new TestHub();

        await EnableAsync(hub);
        await ConnectAsync(hub, Other, Elsewhere);
        await ConnectAsync(hub, Banned, Home);
        await AddAsync(hub, Banned, null, "Wallhack");
        hub.Clock.Advance(TimeSpan.FromHours(1));

        Assert.AreEqual(1, await ConnectAsync(hub, Banned, Elsewhere));
        Assert.AreEqual(Other, (await AlertsAsync(hub)).Single().Alert.CommunityId);
    }

    [TestMethod]
    public async Task ABanOfTheGameServerOrAnImportAlertsTheAccountsAlreadyLinked()
    {
        using var hub = new TestHub();

        await EnableAsync(hub);
        await ConnectAsync(hub, Banned, Home);
        await ConnectAsync(hub, Alternate, Home);
        await ConnectAsync(hub, Other, Elsewhere);
        await ConnectAsync(hub, Other + 1, Elsewhere);
        await CaptureAsync(hub, new GameBanCommand { Player = Banned.ToString(), Minutes = 0, Command = "sm_ban" });
        await hub.RunAsync(services => services.GetRequiredService<IBanService>().ImportAsync($"banid 0 {Other}", new BanActor(Admin, "Admin")));

        var alerts = await AlertsAsync(hub);

        CollectionAssert.AreEquivalent(new[] { Alternate, Other + 1 }, alerts.Select(entry => entry.Alert.CommunityId).ToArray());
    }

    [TestMethod]
    public async Task AnIgnoredAlertIsNotRaisedAgainAndBanningTheAccountClosesItsAlerts()
    {
        using var hub = new TestHub();

        await EnableAsync(hub);
        await ConnectAsync(hub, Banned, Home);
        await AddAsync(hub, Banned, null, "Wallhack");
        hub.Clock.Advance(TimeSpan.FromHours(1));
        await ConnectAsync(hub, Alternate, Home);
        await ConnectAsync(hub, Other, Home);

        var alerts = await AlertsAsync(hub);
        var ignored = await hub.RunAsync(services => services.GetRequiredService<IBanAlertService>().IgnoreAsync(alerts.Single(entry => entry.Alert.CommunityId == Alternate).Alert.Id, Admin));

        await ConnectAsync(hub, Alternate, Home);
        await AddAsync(hub, Other, null, "Alt account");

        Assert.IsTrue(ignored);
        Assert.IsEmpty(await AlertsAsync(hub));
        Assert.AreEqual(0, await hub.RunAsync(services => services.GetRequiredService<IBanAlertService>().OpenCountAsync()));
    }

    [TestMethod]
    public async Task TheAlertsGoAwayWhenTheBanEnds()
    {
        using var hub = new TestHub();

        await EnableAsync(hub);
        await ConnectAsync(hub, Banned, Home);
        await AddAsync(hub, Banned, hub.Clock.UtcNow.AddHours(2), "Rage quit");
        hub.Clock.Advance(TimeSpan.FromHours(1));
        await ConnectAsync(hub, Alternate, Home);

        Assert.HasCount(1, await AlertsAsync(hub));

        hub.Clock.Advance(TimeSpan.FromHours(2));

        Assert.IsEmpty(await AlertsAsync(hub));
    }

    [TestMethod]
    public async Task NoAlertIsRaisedWhileTheModuleIsOff()
    {
        using var hub = new TestHub();

        await ConnectAsync(hub, Banned, Home);
        await AddAsync(hub, Banned, null, "Wallhack");
        hub.Clock.Advance(TimeSpan.FromHours(1));

        Assert.AreEqual(0, await ConnectAsync(hub, Alternate, Home));
    }

    private static async Task<int> ConnectAsync(TestHub hub, long communityId, string address)
    {
        await hub.RunAsync(services => services.GetRequiredService<IPlayerConnectionService>().RegisterAsync(new PlayerConnectionCommand { CommunityId = communityId.ToString(), IpAddress = address, Name = $"Player {communityId % 100}" }));

        return await hub.RunAsync(services => services.GetRequiredService<IBanAlertService>().DetectAsync(communityId));
    }

    private static async Task<IReadOnlyList<BanAlertEntry>> AlertsAsync(TestHub hub)
    {
        var page = await hub.RunAsync(services => services.GetRequiredService<IBanAlertService>().PageAsync(1, 20));

        return page.Items;
    }
}
