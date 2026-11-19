using L4D2PlayStats.Hub.Core.Modules.Administrators.Services;
using L4D2PlayStats.Hub.Core.Modules.Live.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Services;
using L4D2PlayStats.Hub.Core.Modules.Live.Settings;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Core.Live;

[TestClass]
public class SiteChatTests
{
    private const long Alice = 76561198000000001;
    private const long Bob = 76561198000000002;
    private const long Admin = 76561198000000100;

    [TestMethod]
    public async Task AMessageFromTheSiteReachesOnlyItsServer()
    {
        using var hub = await HubAsync();

        var first = await hub.CreateServerAsync("First");
        var second = await hub.CreateServerAsync("Second");

        var result = await SendAsync(hub, first.Server, Alice, "hello");

        var firstState = State(hub, first.Server);
        var secondState = State(hub, second.Server);

        Assert.IsTrue(result.Success);
        Assert.HasCount(1, firstState.SiteMessages);
        Assert.AreEqual("hello", firstState.SiteMessages[0].Text);
        Assert.AreEqual(Alice.ToString(), firstState.SiteMessages[0].CommunityId);
        Assert.HasCount(1, firstState.Feed);
        Assert.AreEqual(LiveTeam.External, ((LiveChatMessage)firstState.Feed[0]).Team);
        Assert.IsEmpty(secondState.SiteMessages);
        Assert.IsEmpty(secondState.Feed);
    }

    [TestMethod]
    public async Task TheGameServerReadsOnlyTheMessagesItHasNotSeen()
    {
        using var hub = await HubAsync();

        var credentials = await hub.CreateServerAsync();

        await SendAsync(hub, credentials.Server, Alice, "first");
        hub.Clock.Advance(TimeSpan.FromMinutes(1));
        await SendAsync(hub, credentials.Server, Alice, "second");

        var state = State(hub, credentials.Server);
        var all = state.SiteMessagesAfter(0);
        var newer = state.SiteMessagesAfter(long.Parse(all[0].Ticks));

        Assert.HasCount(2, all);
        Assert.HasCount(1, newer);
        Assert.AreEqual("second", newer[0].Text);
    }

    [TestMethod]
    public async Task EverybodyWaitsAfterAnyMessage()
    {
        using var hub = await HubAsync();

        var credentials = await hub.CreateServerAsync();

        var first = await SendAsync(hub, credentials.Server, Alice, "hello");

        hub.Clock.Advance(TimeSpan.FromSeconds(CoreConsts.SiteChatGlobalCooldownInSeconds - 1));
        var tooSoon = await SendAsync(hub, credentials.Server, Bob, "hi");

        hub.Clock.Advance(TimeSpan.FromSeconds(1));
        var allowed = await SendAsync(hub, credentials.Server, Bob, "hi");

        Assert.IsTrue(first.Success);
        Assert.AreEqual(SiteChatError.RateLimited, tooSoon.Error);
        Assert.IsTrue(allowed.Success);
    }

    [TestMethod]
    public async Task TheSamePlayerWaitsLongerBetweenMessages()
    {
        using var hub = await HubAsync();

        var credentials = await hub.CreateServerAsync();

        await SendAsync(hub, credentials.Server, Alice, "one");

        hub.Clock.Advance(TimeSpan.FromSeconds(CoreConsts.SiteChatUserCooldownInSeconds - 1));
        var tooSoon = await SendAsync(hub, credentials.Server, Alice, "two");

        hub.Clock.Advance(TimeSpan.FromSeconds(1));
        var allowed = await SendAsync(hub, credentials.Server, Alice, "two");

        Assert.AreEqual(SiteChatError.RateLimited, tooSoon.Error);
        Assert.IsTrue(allowed.Success);
    }

    [TestMethod]
    public async Task TheAdministratorSetsThePausesAndTheLength()
    {
        using var hub = await HubAsync();

        var credentials = await hub.CreateServerAsync();

        await hub.RunAsync(services => services.GetRequiredService<ISettingsService>().SaveAsync(new SiteChatSettings { GlobalCooldownInSeconds = 0, UserCooldownInSeconds = 5, MaxMessageLength = 10 }));

        var first = await SendAsync(hub, credentials.Server, Alice, "one");
        var other = await SendAsync(hub, credentials.Server, Bob, "two");
        var tooSoon = await SendAsync(hub, credentials.Server, Alice, "three");
        var tooLong = await SendAsync(hub, credentials.Server, Bob, "a message longer than ten");

        hub.Clock.Advance(TimeSpan.FromSeconds(5));

        var allowed = await SendAsync(hub, credentials.Server, Alice, "three");

        Assert.IsTrue(first.Success);
        Assert.IsTrue(other.Success);
        Assert.AreEqual(SiteChatError.RateLimited, tooSoon.Error);
        Assert.AreEqual(SiteChatError.MessageTooLong, tooLong.Error);
        Assert.IsTrue(allowed.Success);
    }

    [TestMethod]
    public void TheLimitsStayWithinTheirRanges()
    {
        var settings = new SiteChatSettings { GlobalCooldownInSeconds = -1, UserCooldownInSeconds = 999999, MaxMessageLength = 5000 };

        settings.Normalize();

        Assert.AreEqual(0, settings.GlobalCooldownInSeconds);
        Assert.AreEqual(CoreConsts.SiteChatMaxCooldownInSeconds, settings.UserCooldownInSeconds);
        Assert.AreEqual(CoreConsts.SiteChatMaxMessageLength, settings.MaxMessageLength);
    }

    [TestMethod]
    public async Task TheCooldownOfOneServerDoesNotHoldAnother()
    {
        using var hub = await HubAsync();

        var first = await hub.CreateServerAsync("First");
        var second = await hub.CreateServerAsync("Second");

        var onFirst = await SendAsync(hub, first.Server, Alice, "hello");
        var onSecond = await SendAsync(hub, second.Server, Alice, "hello");

        Assert.IsTrue(onFirst.Success);
        Assert.IsTrue(onSecond.Success);
    }

    [TestMethod]
    public async Task AnAdministratorSkipsTheCooldown()
    {
        using var hub = await HubAsync();

        var credentials = await hub.CreateServerAsync();

        var first = await SendAsync(hub, credentials.Server, Admin, "one");
        var second = await SendAsync(hub, credentials.Server, Admin, "two");

        Assert.IsTrue(first.Success);
        Assert.IsTrue(second.Success);
    }

    [TestMethod]
    public async Task ABannedPlayerCannotWrite()
    {
        using var hub = await HubAsync();

        var credentials = await hub.CreateServerAsync();

        await hub.RunAsync(services => services.GetRequiredService<IChatBanService>().SaveAsync(Alice, "Alice", "Spam", Admin, "Admin"));

        var banned = await SendAsync(hub, credentials.Server, Alice, "hello");

        await hub.RunAsync(services => services.GetRequiredService<IChatBanService>().RemoveAsync(Alice));

        var unbanned = await SendAsync(hub, credentials.Server, Alice, "hello");

        Assert.AreEqual(SiteChatError.Banned, banned.Error);
        Assert.AreEqual("Spam", banned.Reason);
        Assert.IsTrue(unbanned.Success);
    }

    [TestMethod]
    public async Task ABanIsValidOnEveryServer()
    {
        using var hub = await HubAsync();

        var first = await hub.CreateServerAsync("First");
        var second = await hub.CreateServerAsync("Second");

        await hub.RunAsync(services => services.GetRequiredService<IChatBanService>().SaveAsync(Alice, "Alice", null, Admin, "Admin"));

        var onFirst = await SendAsync(hub, first.Server, Alice, "hello");
        var onSecond = await SendAsync(hub, second.Server, Alice, "hello");

        Assert.AreEqual(SiteChatError.Banned, onFirst.Error);
        Assert.AreEqual(SiteChatError.Banned, onSecond.Error);
    }

    [TestMethod]
    public async Task AnEmptyOrTooLongMessageIsRefused()
    {
        using var hub = await HubAsync();

        var credentials = await hub.CreateServerAsync();

        var empty = await SendAsync(hub, credentials.Server, Alice, "  ");
        var tooLong = await SendAsync(hub, credentials.Server, Alice, new string('a', CoreConsts.SiteChatMaxMessageLength + 1));
        var longest = await SendAsync(hub, credentials.Server, Alice, new string('a', CoreConsts.SiteChatMaxMessageLength));

        Assert.AreEqual(SiteChatError.EmptyMessage, empty.Error);
        Assert.AreEqual(SiteChatError.MessageTooLong, tooLong.Error);
        Assert.IsTrue(longest.Success);
    }

    [TestMethod]
    public async Task NothingIsSentWhileTheModuleIsOff()
    {
        using var hub = await HubAsync();

        var credentials = await hub.CreateServerAsync();

        await hub.RunAsync(services => services.GetRequiredService<ISettingsService>().SaveAsync(new ModuleSettings { SiteChat = false }));

        var result = await SendAsync(hub, credentials.Server, Alice, "hello");

        Assert.AreEqual(SiteChatError.Disabled, result.Error);
        Assert.IsEmpty(State(hub, credentials.Server).SiteMessages);
    }

    [TestMethod]
    public async Task SiteMessagesAreForgottenAfterOneHour()
    {
        using var hub = await HubAsync();

        var credentials = await hub.CreateServerAsync();

        await SendAsync(hub, credentials.Server, Alice, "hello");

        hub.Clock.Advance(TimeSpan.FromMinutes(CoreConsts.SiteChatRetentionInMinutes));

        Assert.IsEmpty(State(hub, credentials.Server).SiteMessages);
    }

    [TestMethod]
    public async Task TheLatestSendersAreListedForTheAdministrator()
    {
        using var hub = await HubAsync();

        var first = await hub.CreateServerAsync("First");
        var second = await hub.CreateServerAsync("Second");

        await SendAsync(hub, first.Server, Alice, "one");
        hub.Clock.Advance(TimeSpan.FromMinutes(1));
        await SendAsync(hub, first.Server, Alice, "two");
        await SendAsync(hub, second.Server, Bob, "three");
        await hub.RunAsync(services => services.GetRequiredService<IChatBanService>().SaveAsync(Bob, "Bob", null, Admin, "Admin"));

        var senders = await hub.RunAsync(services => services.GetRequiredService<ISiteChatService>().RecentSendersAsync());

        Assert.HasCount(2, senders);
        Assert.AreEqual("two", senders.Single(sender => sender.CommunityId == Alice).Text);
        Assert.IsFalse(senders.Single(sender => sender.CommunityId == Alice).Banned);
        Assert.IsTrue(senders.Single(sender => sender.CommunityId == Bob).Banned);
    }

    private static async Task<TestHub> HubAsync()
    {
        var hub = new TestHub();

        await hub.RunAsync(async services =>
        {
            await services.GetRequiredService<ISettingsService>().SaveAsync(new ModuleSettings { SiteChat = true });
            await services.GetRequiredService<IAdministratorService>().AddAsync(Admin, "Admin");
        });

        return hub;
    }

    private static Task<SiteChatResult> SendAsync(TestHub hub, GameServer server, long communityId, string message)
    {
        return hub.RunAsync(services => services.GetRequiredService<ISiteChatService>().SendAsync(server, new SiteChatUser(communityId, $"Player {communityId}"), message));
    }

    private static LiveServerState State(TestHub hub, GameServer server)
    {
        using var scope = hub.Scope();

        return scope.ServiceProvider.GetRequiredService<ILiveServerStore>().For(server.Id);
    }
}
