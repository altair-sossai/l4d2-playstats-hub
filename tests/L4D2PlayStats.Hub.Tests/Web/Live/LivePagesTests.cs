using System.Net;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;
using L4D2PlayStats.Hub.Core.Modules.Live.Services;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web.Live;

[TestClass]
public class LivePagesTests
{
    [TestMethod]
    public async Task TheServerPagesOpenWithOneServer()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();
        await factory.CreateServerAsync();

        var list = await client.GetAsync("/servers");
        var live = await client.GetAsync("/servers/test-server");
        var body = await live.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, list.StatusCode);
        Assert.Contains("Test Server", await list.Content.ReadAsStringAsync());
        Assert.AreEqual(HttpStatusCode.OK, live.StatusCode);
        Assert.Contains("live-header", body);
        Assert.Contains("feed-poller", body);
        Assert.DoesNotContain("server-picker", body);
    }

    [TestMethod]
    public async Task WithTwoServersTheVisitorPicksWhichOneToWatch()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();
        await factory.CreateServerAsync("First");
        await factory.CreateServerAsync("Second");

        var list = await client.GetAsync("/servers");
        var listBody = await list.Content.ReadAsStringAsync();
        var first = await client.GetAsync("/servers/first");
        var second = await client.GetAsync("/servers/second");
        var body = await second.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, list.StatusCode);
        Assert.Contains("/servers/first", listBody);
        Assert.Contains("/servers/second", listBody);
        Assert.AreEqual(HttpStatusCode.OK, first.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, second.StatusCode);
        Assert.Contains("server-picker", body);
        Assert.Contains("<option value=\"first\">First</option>", body);
        Assert.Contains("<option value=\"second\" selected=\"selected\">Second</option>", body);
    }

    [TestMethod]
    public async Task AnUnknownOrDisabledServerIsNotFound()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();

        await factory.RunAsync(services => services.GetRequiredService<IGameServerService>().UpdateAsync(credentials.Server.Id, "Test Server", null, false));

        var unknown = await client.GetAsync("/servers/nothing-here");
        var disabled = await client.GetAsync("/servers/test-server");
        var list = await client.GetStringAsync("/servers");

        Assert.AreEqual(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, disabled.StatusCode);
        Assert.DoesNotContain("Test Server", list);
    }

    [TestMethod]
    public async Task TheLivePageShowsTheMatchOfItsServerOnly()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var first = await factory.CreateServerAsync("First");
        var second = await factory.CreateServerAsync("Second");

        await factory.RunAsync(services =>
        {
            var live = services.GetRequiredService<ILiveDataService>();

            live.UpdateConfiguration(first.Server, new LiveConfiguration { Name = "ZoneMod v2.9", TeamSize = 4 });
            live.UpdateRound(first.Server, new LiveRound { TankPercent = 0.62m, InSecondHalfOfRound = true });
            live.UpdateScoreboard(first.Server, new LiveScoreboard { SurvivorScore = 812, InfectedScore = 640, IsTankInPlay = true });
            live.UpdatePlayers(first.Server, new PlayersCommand
            {
                Survivors = [new LiveSurvivor { CommunityId = "76561198000000001", Name = "AliceTheSurvivor", IsPlayerAlive = true, PermanentHealth = 80 }],
                Infecteds = [new LiveInfected { CommunityId = "76561198000000002", Name = "BobTheHunter", IsPlayerAlive = true, Health = 250, MaxHealth = 250 }],
                Spectators = [new LivePlayer { CommunityId = "76561198000000003", Name = "CarolTheSpectator" }]
            });

            foreach (var gameEvent in EveryEvent())
                live.AddEvent(first.Server, gameEvent);

            live.AddMessage(first.Server, new ChatMessageCommand { CommunityId = "76561198000000001", Name = "AliceTheSurvivor", Message = "tank incoming" });

            return Task.CompletedTask;
        });

        factory.PassTheDelay();

        var firstPage = await client.GetStringAsync("/servers/first");
        var secondPage = await client.GetStringAsync("/servers/second");
        var header = await client.GetStringAsync("/servers/first/header");
        var players = await client.GetStringAsync("/servers/first/players");
        var cards = await client.GetStringAsync("/servers");

        Assert.Contains("ZoneMod v2.9", firstPage);
        Assert.Contains("AliceTheSurvivor", firstPage);
        Assert.Contains("BobTheHunter", firstPage);
        Assert.Contains("CarolTheSpectator", firstPage);
        Assert.Contains("tank incoming", firstPage);
        Assert.Contains("crowned a witch", firstPage);
        Assert.Contains("Unknown game event.", firstPage);
        Assert.DoesNotContain("ZoneMod v2.9", secondPage);
        Assert.DoesNotContain("AliceTheSurvivor", secondPage);
        Assert.DoesNotContain("tank incoming", secondPage);
        Assert.Contains("812", header);
        Assert.Contains("Second round", header);
        Assert.Contains("AliceTheSurvivor", players);
        Assert.Contains("ZoneMod v2.9", cards);
    }

    [TestMethod]
    public async Task ThePollingFeedBringsOnlyWhatIsNew()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();
        var state = factory.Services.GetRequiredService<ILiveServerStore>().For(credentials.Server.Id);

        state.AddEvent(new MixStartedEvent());
        factory.PassTheDelay();

        var first = await client.GetStringAsync("/servers/test-server/feed?after=0");

        state.AddEvent(new TankSpawnedEvent { Actor = new LivePlayer { Name = "BobTheTank" } });
        factory.PassTheDelay();

        var second = await client.GetStringAsync("/servers/test-server/feed?after=1");
        var third = await client.GetStringAsync("/servers/test-server/feed?after=2");

        Assert.Contains("A mix has started.", first);
        Assert.Contains("feed?after=1", first);
        Assert.Contains("hx-swap-oob", first);
        Assert.DoesNotContain("A mix has started.", second);
        Assert.Contains("BobTheTank", second);
        Assert.Contains("feed?after=2", second);
        Assert.Contains("feed?after=2", third);
        Assert.DoesNotContain("hx-swap-oob", third);
    }

    [TestMethod]
    public async Task TheConnectLinkAppearsWhenTheServerHasAnAddress()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();
        await factory.CreateServerAsync("Hidden");
        await factory.RunAsync(services => services.GetRequiredService<IGameServerService>().CreateAsync("Public", "play.example.com:27015"));

        var list = await client.GetStringAsync("/servers");
        var hidden = await client.GetStringAsync("/servers/hidden");
        var visible = await client.GetStringAsync("/servers/public");

        Assert.Contains("steam://connect/play.example.com:27015", list);
        Assert.Contains("steam://connect/play.example.com:27015", visible);
        Assert.DoesNotContain("steam://connect/", hidden);
    }

    private static GameEvent[] EveryEvent()
    {
        var alice = new LivePlayer { CommunityId = "76561198000000001", Name = "AliceTheSurvivor" };
        var bob = new LivePlayer { CommunityId = "76561198000000002", Name = "BobTheHunter" };

        return
        [
            new BoomerVomitLandedEvent { Actor = bob, Amount = 3 },
            new CarAlarmTriggeredEvent { Actor = alice, Infected = bob },
            new ChargerLevelEvent { Actor = alice, Charger = bob },
            new ChargerLevelHurtEvent { Actor = alice, Charger = bob, Damage = 100 },
            new DeathChargeEvent { Actor = bob, Victim = alice, Height = 500 },
            new HunterHighPounceEvent { Actor = bob, Victim = alice, CalculatedDamage = 25, Height = 600 },
            new MixStartedEvent(),
            new PauseEvent { Paused = true },
            new PlayerDeathEvent { Actor = alice },
            new RoundEndedEvent { SurvivorScore = 500, InfectedScore = 300 },
            new RoundLiveEvent { SecondHalf = true },
            new SkeetEvent { Actor = alice, Hunter = bob },
            new SkeetEvent { Hunter = bob, IsTeamSkeet = true },
            new SkeetHurtEvent { Actor = alice, Hunter = bob, Damage = 120, IsOverkill = true },
            new TankBecameBotEvent(),
            new TankDiedEvent { Actor = bob },
            new TankRockEatenEvent { Actor = bob, Victim = alice },
            new TankRockSkeetedEvent { Actor = alice, Tank = bob },
            new TankSpawnedEvent { Actor = bob },
            new TongueCutEvent { Actor = alice, Smoker = bob },
            new WitchCrownEvent { Actor = alice, Damage = 1000 },
            new WitchCrownHurtEvent { Actor = alice, Damage = 900, ChipDamage = 100 },
            new GameEvent()
        ];
    }
}
