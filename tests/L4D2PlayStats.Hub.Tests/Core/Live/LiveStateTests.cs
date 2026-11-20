using L4D2PlayStats.Hub.Core.Modules.Live.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Servers.Enums;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Core.Live;

[TestClass]
public class LiveStateTests
{
    private static readonly TimeSpan Delay = TimeSpan.FromSeconds(CoreConsts.LiveDelayInSeconds);

    [TestMethod]
    public async Task TheStateOfTwoServersNeverMixes()
    {
        using var hub = new TestHub();
        using var scope = hub.Scope();

        var live = scope.ServiceProvider.GetRequiredService<ILiveDataService>();
        var first = await hub.CreateServerAsync("First");
        var second = await hub.CreateServerAsync("Second");

        live.UpdateConfiguration(first.Server, new LiveConfiguration { Name = "ZoneMod", TeamSize = 4 });
        live.UpdateScoreboard(first.Server, new LiveScoreboard { SurvivorScore = 100, InfectedScore = 50 });
        live.UpdatePlayers(first.Server, new PlayersCommand { Spectators = [new LivePlayer { CommunityId = "76561198000000001", Name = "Alice" }] });
        live.AddMessage(first.Server, new ChatMessageCommand { CommunityId = "76561198000000001", Name = "Alice", Message = "hello", Team = LiveTeam.Survivor });
        live.UpdateConfiguration(second.Server, new LiveConfiguration { Name = "EQ", TeamSize = 3 });

        hub.Clock.Advance(Delay);

        var firstSnapshot = live.Snapshot(first.Server);
        var secondSnapshot = live.Snapshot(second.Server);

        Assert.AreEqual("ZoneMod", firstSnapshot.Configuration!.Name);
        Assert.AreEqual(100, firstSnapshot.Scoreboard!.SurvivorScore);
        Assert.HasCount(1, firstSnapshot.Spectators);
        Assert.HasCount(1, firstSnapshot.Messages);
        Assert.AreEqual("EQ", secondSnapshot.Configuration!.Name);
        Assert.IsNull(secondSnapshot.Scoreboard);
        Assert.IsEmpty(secondSnapshot.Spectators);
        Assert.IsEmpty(secondSnapshot.Messages);
    }

    [TestMethod]
    public async Task LiveDataIsPublishedOnlyAfterTheDelay()
    {
        using var hub = new TestHub();
        using var scope = hub.Scope();

        var live = scope.ServiceProvider.GetRequiredService<ILiveDataService>();
        var credentials = await hub.CreateServerAsync();

        live.UpdateRound(credentials.Server, new LiveRound { TankPercent = 0.5m });

        hub.Clock.Advance(Delay - TimeSpan.FromSeconds(1));
        var before = live.Snapshot(credentials.Server).Round;

        hub.Clock.Advance(TimeSpan.FromSeconds(1));
        var after = live.Snapshot(credentials.Server).Round;

        Assert.IsNull(before);
        Assert.AreEqual(0.5m, after!.TankPercent);
    }

    [TestMethod]
    public async Task ValuesArePublishedInTheOrderTheyArrived()
    {
        using var hub = new TestHub();
        using var scope = hub.Scope();

        var live = scope.ServiceProvider.GetRequiredService<ILiveDataService>();
        var credentials = await hub.CreateServerAsync();

        live.UpdateScoreboard(credentials.Server, new LiveScoreboard { SurvivorScore = 1 });
        hub.Clock.Advance(TimeSpan.FromSeconds(5));
        live.UpdateScoreboard(credentials.Server, new LiveScoreboard { SurvivorScore = 2 });

        hub.Clock.Advance(TimeSpan.FromSeconds(10));
        var first = live.Snapshot(credentials.Server).Scoreboard;

        hub.Clock.Advance(TimeSpan.FromSeconds(5));
        var second = live.Snapshot(credentials.Server).Scoreboard;

        Assert.AreEqual(1, first!.SurvivorScore);
        Assert.AreEqual(2, second!.SurvivorScore);
    }

    [TestMethod]
    public async Task TheScoreboardExpiresBeforeTheRestOfTheMatch()
    {
        using var hub = new TestHub();
        using var scope = hub.Scope();

        var live = scope.ServiceProvider.GetRequiredService<ILiveDataService>();
        var credentials = await hub.CreateServerAsync();

        live.UpdateScoreboard(credentials.Server, new LiveScoreboard { SurvivorScore = 10 });
        live.UpdateConfiguration(credentials.Server, new LiveConfiguration { Name = "ZoneMod", TeamSize = 4 });
        live.UpdatePlayers(credentials.Server, new PlayersCommand { Survivors = [new LiveSurvivor { CommunityId = "76561198000000001", Name = "Alice" }] });

        hub.Clock.Advance(Delay + TimeSpan.FromMinutes(CoreConsts.LiveScoreboardExpirationInMinutes) - TimeSpan.FromSeconds(1));
        var alive = live.Snapshot(credentials.Server);

        hub.Clock.Advance(TimeSpan.FromSeconds(1));
        var scoreboardExpired = live.Snapshot(credentials.Server);

        hub.Clock.Advance(TimeSpan.FromMinutes(CoreConsts.LivePlayersExpirationInMinutes));
        var everythingExpired = live.Snapshot(credentials.Server);

        Assert.IsNotNull(alive.Scoreboard);
        Assert.IsNull(scoreboardExpired.Scoreboard);
        Assert.IsNotNull(scoreboardExpired.Configuration);
        Assert.HasCount(1, scoreboardExpired.Survivors);
        Assert.IsNull(everythingExpired.Configuration);
        Assert.IsEmpty(everythingExpired.Survivors);
        Assert.IsFalse(everythingExpired.AnyPlayerConnected);
    }

    [TestMethod]
    public async Task PlayersAreSortedAndCounted()
    {
        using var hub = new TestHub();
        using var scope = hub.Scope();

        var live = scope.ServiceProvider.GetRequiredService<ILiveDataService>();
        var credentials = await hub.CreateServerAsync();

        live.UpdatePlayers(credentials.Server, new PlayersCommand
        {
            Survivors =
            [
                new LiveSurvivor { CommunityId = "76561198000000002", Name = "Ellis", Character = SurvivorCharacter.Ellis },
                new LiveSurvivor { CommunityId = "76561198000000001", Name = "Nick", Character = SurvivorCharacter.Nick }
            ],
            Infecteds = [new LiveInfected { CommunityId = "76561198000000003", Name = "Hunter", Type = InfectedType.Hunter }],
            Spectators = [new LivePlayer { CommunityId = "not-a-steam-id", Name = "Zed" }, new LivePlayer { CommunityId = "76561198000000004", Name = "Ann" }]
        });

        hub.Clock.Advance(Delay);

        var snapshot = live.Snapshot(credentials.Server);

        Assert.AreEqual(5, snapshot.PlayerCount);
        Assert.AreEqual("Nick", snapshot.Survivors[0].Name);
        Assert.AreEqual("Ann", snapshot.Spectators[0].Name);
        Assert.IsNull(snapshot.Spectators[1].CommunityId);
        Assert.AreEqual("https://steamcommunity.com/profiles/76561198000000004", snapshot.Spectators[0].ProfileUrl);
    }

    [TestMethod]
    public async Task LiveDataFeedsTheServerChecklist()
    {
        using var hub = new TestHub();
        using var scope = hub.Scope();

        var live = scope.ServiceProvider.GetRequiredService<ILiveDataService>();
        var presence = scope.ServiceProvider.GetRequiredService<IGameServerPresence>();
        var credentials = await hub.CreateServerAsync();
        var before = presence.GetStatus(credentials.Server);

        live.UpdateRound(credentials.Server, new LiveRound());

        var after = presence.GetStatus(credentials.Server);

        Assert.IsNull(before.LastLiveDataAtUtc);
        Assert.AreEqual(hub.Clock.UtcNow, after.LastLiveDataAtUtc);
        Assert.AreEqual(GameServerConnectionState.Online, after.State);
    }
}