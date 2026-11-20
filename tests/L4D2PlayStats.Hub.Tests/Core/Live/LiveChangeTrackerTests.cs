using L4D2PlayStats.Hub.Core.Modules.Live.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;
using L4D2PlayStats.Hub.Core.Modules.Servers.Models;
using L4D2PlayStats.Hub.Tests.Support;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Core.Live;

[TestClass]
public class LiveChangeTrackerTests
{
    private const long Alice = 76561198000000001;

    private static readonly TimeSpan Delay = TimeSpan.FromSeconds(CoreConsts.LiveDelayInSeconds);
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(CoreConsts.LiveRefreshInMilliseconds);

    [TestMethod]
    public void TheFirstLookOnlyTakesTheBaseline()
    {
        var clock = new FakeClock();
        var state = new LiveServerState(clock);
        var tracker = new LiveChangeTracker();

        state.Update(new LiveScoreboard { SurvivorScore = 100 });
        clock.Advance(Delay);

        Assert.AreEqual(LiveChange.None, Detect(tracker, state, clock));
        Assert.AreEqual(LiveChange.None, Detect(tracker, state, clock));
    }

    [TestMethod]
    public void PlayersAreReportedOnlyAfterTheDelay()
    {
        var clock = new FakeClock();
        var state = new LiveServerState(clock);
        var tracker = new LiveChangeTracker();

        Detect(tracker, state, clock);

        state.Update(new PlayersCommand { Survivors = [new LiveSurvivor { CommunityId = Alice.ToString(), Name = "Alice", PermanentHealth = 80 }] });

        clock.Advance(Delay - TimeSpan.FromSeconds(1));

        var early = Detect(tracker, state, clock);

        clock.Advance(TimeSpan.FromSeconds(1));

        var published = Detect(tracker, state, clock);
        var again = Detect(tracker, state, clock);

        Assert.AreEqual(LiveChange.None, early);
        Assert.AreEqual(LiveChange.Header | LiveChange.Players, published);
        Assert.AreEqual(LiveChange.None, again);
    }

    [TestMethod]
    public void TheSameDataSentAgainIsNotAChange()
    {
        var clock = new FakeClock();
        var state = new LiveServerState(clock);
        var tracker = new LiveChangeTracker();

        state.Update(new LiveRound { TankPercent = 0.5m });
        clock.Advance(Delay);
        Detect(tracker, state, clock);

        state.Update(new LiveRound { TankPercent = 0.5m });
        clock.Advance(Delay);

        Assert.AreEqual(LiveChange.None, Detect(tracker, state, clock));
    }

    [TestMethod]
    public void GameMessagesAreReportedAfterTheDelayAndSiteMessagesAtOnce()
    {
        var clock = new FakeClock();
        var state = new LiveServerState(clock);
        var tracker = new LiveChangeTracker();

        Detect(tracker, state, clock);

        state.AddMessage(new ChatMessageCommand { CommunityId = Alice.ToString(), Name = "Alice", Message = "tank incoming" });
        state.AddEvent(new TankSpawnedEvent());

        var early = Detect(tracker, state, clock);

        clock.Advance(Delay);

        var published = Detect(tracker, state, clock);

        clock.Advance(Interval);
        state.AddSiteMessage(new SiteChatUser(Alice, "Alice"), "hello from the site", true);

        var site = Detect(tracker, state, clock);

        Assert.AreEqual(LiveChange.None, early);
        Assert.AreEqual(LiveChange.Feed, published);
        Assert.AreEqual(LiveChange.Feed, site);
    }

    [TestMethod]
    public void ChangesAreReportedAtMostOncePerRefreshInterval()
    {
        var clock = new FakeClock();
        var state = new LiveServerState(clock);
        var tracker = new LiveChangeTracker();

        Detect(tracker, state, clock);

        state.Update(new LiveScoreboard { SurvivorScore = 100 });
        clock.Advance(TimeSpan.FromSeconds(1));
        state.Update(new LiveScoreboard { SurvivorScore = 200 });
        clock.Advance(Delay - TimeSpan.FromSeconds(1));

        var first = Detect(tracker, state, clock);

        clock.Advance(TimeSpan.FromSeconds(1));

        var throttled = Detect(tracker, state, clock);

        clock.Advance(Interval);

        var later = Detect(tracker, state, clock);

        Assert.AreEqual(LiveChange.Header | LiveChange.Players, first);
        Assert.AreEqual(LiveChange.None, throttled);
        Assert.AreEqual(LiveChange.Header | LiveChange.Players, later);
    }

    [TestMethod]
    public void TheServerNameAndMapAreAHeaderChange()
    {
        var clock = new FakeClock();
        var state = new LiveServerState(clock);
        var tracker = new LiveChangeTracker();

        tracker.Detect(state, new GameServerSignal { Map = "c2m1_highway" }, clock.UtcNow, Interval, Interval / 2);

        var same = tracker.Detect(state, new GameServerSignal { Map = "c2m1_highway", ReceivedAtUtc = clock.UtcNow.AddSeconds(5) }, clock.UtcNow, Interval, Interval / 2);
        var other = tracker.Detect(state, new GameServerSignal { Map = "c2m2_fairgrounds" }, clock.UtcNow, Interval, Interval / 2);

        Assert.AreEqual(LiveChange.None, same);
        Assert.AreEqual(LiveChange.Header, other);
    }

    private static LiveChange Detect(LiveChangeTracker tracker, LiveServerState state, FakeClock clock)
    {
        return tracker.Detect(state, null, clock.UtcNow, Interval, Interval / 2);
    }
}