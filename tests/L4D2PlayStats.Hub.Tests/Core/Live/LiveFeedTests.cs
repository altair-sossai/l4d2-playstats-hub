using L4D2PlayStats.Hub.Core.Modules.Live.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Core.Live;

[TestClass]
public class LiveFeedTests
{
    private const long Alice = 76561198000000001;

    private static readonly TimeSpan Delay = TimeSpan.FromSeconds(CoreConsts.LiveDelayInSeconds);

    [TestMethod]
    public void EventsAndMessagesAppearAfterTheDelay()
    {
        using var hub = new TestHub();

        var state = State(hub);

        state.AddEvent(new TankSpawnedEvent());
        state.AddMessage(Message("hello"));

        var before = state.Feed;

        hub.Clock.Advance(Delay);

        var after = state.Feed;

        Assert.IsEmpty(before);
        Assert.HasCount(2, after);
        Assert.IsInstanceOfType<TankSpawnedEvent>(after[0]);
        Assert.IsInstanceOfType<LiveChatMessage>(after[1]);
    }

    [TestMethod]
    public void TheFeedKeepsOnlyTheLatestItems()
    {
        using var hub = new TestHub();

        var state = State(hub);

        for (var index = 0; index < CoreConsts.LiveFeedMaxItems + 10; index++)
        {
            state.AddEvent(new WitchCrownEvent { Damage = index });
            hub.Clock.Advance(TimeSpan.FromSeconds(1));
        }

        hub.Clock.Advance(Delay);

        var feed = state.Feed;

        Assert.HasCount(CoreConsts.LiveFeedMaxItems, feed);
        Assert.AreEqual(10, ((WitchCrownEvent)feed[0]).Damage);
        Assert.AreEqual(CoreConsts.LiveFeedMaxItems + 9, ((WitchCrownEvent)feed[^1]).Damage);
    }

    [TestMethod]
    public void FeedItemsExpireOneHourAfterTheyWerePublished()
    {
        using var hub = new TestHub();

        var state = State(hub);

        state.AddEvent(new MixStartedEvent());

        hub.Clock.Advance(Delay + TimeSpan.FromMinutes(CoreConsts.LiveFeedRetentionInMinutes) - TimeSpan.FromSeconds(1));
        var alive = state.Feed;

        hub.Clock.Advance(TimeSpan.FromSeconds(1));
        var expired = state.Feed;

        Assert.HasCount(1, alive);
        Assert.IsEmpty(expired);
    }

    [TestMethod]
    public void ARepeatedMessageOrAChatBindIsIgnored()
    {
        using var hub = new TestHub();

        var state = State(hub);

        var first = state.AddMessage(Message("Nice shot"));
        var repeated = state.AddMessage(Message("nice SHOT"));
        var bind = state.AddMessage(Message("go go go!"));
        var empty = state.AddMessage(Message("   "));
        var different = state.AddMessage(Message("thanks"));

        hub.Clock.Advance(Delay);

        Assert.IsTrue(first);
        Assert.IsFalse(repeated);
        Assert.IsFalse(bind);
        Assert.IsFalse(empty);
        Assert.IsTrue(different);
        Assert.HasCount(2, state.Feed);
    }

    [TestMethod]
    public void TheStarsOfAnEventFollowItsKind()
    {
        var skeet = new SkeetEvent { Hunter = new LivePlayer { Name = "Hunter" } };
        var teamSkeet = new SkeetEvent { IsTeamSkeet = true };
        var botSkeet = new SkeetEvent { Hunter = new LivePlayer { Name = "Hunter", IsBot = true } };

        Assert.AreEqual(2, skeet.Stars);
        Assert.AreEqual(1, teamSkeet.Stars);
        Assert.AreEqual(1, botSkeet.Stars);
        Assert.AreEqual(4, new DeathChargeEvent().Stars);
        Assert.AreEqual(0, new TankDiedEvent().Stars);
    }

    [TestMethod]
    public void AnItemPublishedLateStillReachesAVisitorWhoIsPolling()
    {
        using var hub = new TestHub();

        var state = State(hub);

        state.AddEvent(new TankSpawnedEvent());
        hub.Clock.Advance(TimeSpan.FromSeconds(5));
        state.AddSiteMessage(new SiteChatUser(Alice, "Alice"), "hello from the site", false);

        var seen = state.FeedAfter(0);
        var last = seen.Max(item => item.Sequence);

        hub.Clock.Advance(Delay);

        var late = state.FeedAfter(last);
        var feed = state.Feed;

        Assert.HasCount(1, seen);
        Assert.IsInstanceOfType<LiveChatMessage>(seen[0]);
        Assert.HasCount(1, late);
        Assert.IsInstanceOfType<TankSpawnedEvent>(late[0]);
        Assert.IsInstanceOfType<TankSpawnedEvent>(feed[0]);
        Assert.HasCount(2, state.FeedAfter(long.MaxValue));
    }

    private static LiveServerState State(TestHub hub)
    {
        using var scope = hub.Scope();

        return scope.ServiceProvider.GetRequiredService<ILiveServerStore>().For(1);
    }

    private static ChatMessageCommand Message(string text)
    {
        return new ChatMessageCommand { CommunityId = Alice.ToString(), Name = "Alice", Message = text, Team = LiveTeam.Survivor, Public = true };
    }
}
