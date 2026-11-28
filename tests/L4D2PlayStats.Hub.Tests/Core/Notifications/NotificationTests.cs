using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Administrators.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Enums;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Models;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static L4D2PlayStats.Hub.Tests.Core.Championships.ChampionshipFixture;

namespace L4D2PlayStats.Hub.Tests.Core.Notifications;

[TestClass]
public class NotificationTests
{
    private const long Player = 76561198100000500;
    private const long Other = 76561198100000501;

    [TestMethod]
    public async Task EachAudienceSeesOnlyItsNotifications()
    {
        using var hub = new TestHub();

        await SendAsync(hub, notifications =>
        {
            notifications.Send(Player, Message("For the player"));
            notifications.SendAdministrators(Message("For the administrators"));
            notifications.SendEveryone(Message("For everyone"));
        });

        CollectionAssert.AreEquivalent(new[] { "For the player", "For everyone" }, (await LatestAsync(hub, Player, false)).Select(item => item.Text).ToArray());
        CollectionAssert.AreEquivalent(new[] { "For everyone" }, (await LatestAsync(hub, Other, false)).Select(item => item.Text).ToArray());
        CollectionAssert.AreEquivalent(new[] { "For the administrators", "For everyone" }, (await LatestAsync(hub, Admin, true)).Select(item => item.Text).ToArray());
        Assert.AreEqual(2, await UnreadAsync(hub, Player, false));
    }

    [TestMethod]
    public async Task OpeningMarksOneAsReadAndReadAllMarksTheRestForThatPlayerOnly()
    {
        using var hub = new TestHub();

        await SendAsync(hub, notifications =>
        {
            notifications.Send(Player, new NotificationMessage(NotificationTopic.Skins, "You received the skin {0} as a gift. It is already equipped.", "/skins", "AK-47"));
            notifications.Send(Other, Message("Not yours"));
            notifications.SendEveryone(Message("For everyone"));
            notifications.SendEveryone(Message("Also for everyone"));
        });

        var mine = (await LatestAsync(hub, Player, false)).First(item => item.Topic == NotificationTopic.Skins);
        var hidden = (await LatestAsync(hub, Other, false)).First(item => item.Text == "Not yours");

        Assert.IsNull(await hub.RunAsync(services => services.GetRequiredService<INotificationService>().OpenAsync(Player, false, hidden.Id)));

        var opened = await hub.RunAsync(services => services.GetRequiredService<INotificationService>().OpenAsync(Player, false, mine.Id));

        Assert.IsNotNull(opened);
        Assert.AreEqual("/skins", opened.Url);
        CollectionAssert.AreEqual(new[] { "AK-47" }, opened.Values);
        Assert.AreEqual(2, await UnreadAsync(hub, Player, false));

        await hub.RunAsync(services => services.GetRequiredService<INotificationService>().OpenAsync(Player, false, mine.Id));
        Assert.AreEqual(2, await hub.RunAsync(services => services.GetRequiredService<INotificationService>().ReadAllAsync(Player, false)));
        Assert.AreEqual(0, await UnreadAsync(hub, Player, false));
        Assert.IsTrue((await LatestAsync(hub, Player, false)).All(item => item.Read));
        Assert.AreEqual(3, await UnreadAsync(hub, Other, false));
    }

    [TestMethod]
    public async Task ThePagesGoFromTheNewestToTheOldest()
    {
        using var hub = new TestHub();

        for (var index = 1; index <= 5; index++)
        {
            var text = $"Number {index}";

            await SendAsync(hub, notifications => notifications.Send(Player, Message(text)));
            hub.Clock.Advance(TimeSpan.FromMinutes(1));
        }

        var first = await hub.RunAsync(services => services.GetRequiredService<INotificationService>().PageAsync(Player, false, 1, 2));
        var last = await hub.RunAsync(services => services.GetRequiredService<INotificationService>().PageAsync(Player, false, 3, 2));

        Assert.AreEqual(5, first.Total);
        CollectionAssert.AreEqual(new[] { "Number 5", "Number 4" }, first.Items.Select(item => item.Text).ToArray());
        CollectionAssert.AreEqual(new[] { "Number 1" }, last.Items.Select(item => item.Text).ToArray());
    }

    [TestMethod]
    public async Task TheCleanupDeletesTheOldNotificationsAndWhoReadThem()
    {
        using var hub = new TestHub();

        await SendAsync(hub, notifications => notifications.SendEveryone(Message("Old")));
        await hub.RunAsync(services => services.GetRequiredService<INotificationService>().ReadAllAsync(Player, false));

        hub.Clock.Advance(TimeSpan.FromDays(80));
        await SendAsync(hub, notifications => notifications.SendEveryone(Message("Recent")));
        hub.Clock.Advance(TimeSpan.FromDays(11));

        Assert.AreEqual(1, await hub.RunAsync(services => services.GetRequiredService<INotificationService>().CleanupAsync()));
        CollectionAssert.AreEqual(new[] { "Recent" }, (await LatestAsync(hub, Player, false)).Select(item => item.Text).ToArray());
        Assert.AreEqual(0, await hub.RunAsync(services => services.GetRequiredService<HubDbContext>().NotificationReads.CountAsync()));
    }

    [TestMethod]
    public async Task ANewChampionshipAndItsRequestsToJoinNotifyTheRightPeople()
    {
        using var hub = new TestHub();

        var championship = await CreateAsync(hub, "Open cup", Knockout());

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipService>().UpdateAsync(championship.Id, new ChampionshipSettingsForm { Name = "Open cup", JoinRequests = true }, Admin));

        Assert.IsEmpty(await LatestAsync(hub, Player, false));

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipService>().SetStatusAsync(championship.Id, ChampionshipStatus.Participants, null, Admin));

        var news = (await LatestAsync(hub, Other, false)).Single();

        Assert.AreEqual("There is a new championship you can join: {0}.", news.Text);
        CollectionAssert.AreEqual(new[] { "Open cup" }, news.Values);
        Assert.AreEqual($"/championships/{championship.Slug}", news.Url);

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipJoinService>().AskAsync(championship.Id, Player, null));
        await hub.RunAsync(services => services.GetRequiredService<IChampionshipJoinService>().CancelAsync(championship.Id, Player));

        var administration = await LatestAsync(hub, Admin, true);

        Assert.IsTrue(administration.Any(item => item.Text == "{0} asked for a place in {1}." && item.Values[0] == Player.ToString() && item.Url!.EndsWith("?tab=participants")));
        Assert.IsTrue(administration.Any(item => item.Text == "{0} cancelled the request to join {1}."));
        Assert.IsFalse((await LatestAsync(hub, Other, false)).Any(item => item.Text.Contains("asked for a place")));

        var request = await hub.RunAsync(services => services.GetRequiredService<IChampionshipJoinService>().AskAsync(championship.Id, Other, null));

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipJoinService>().ApproveAsync(request.Id, Admin));

        Assert.IsTrue((await LatestAsync(hub, Other, false)).Any(item => item.Text == "Your request to join {0} was approved: you are a participant now."));
        Assert.IsFalse((await LatestAsync(hub, Player, false)).Any(item => item.Text.StartsWith("Your request")));
    }

    [TestMethod]
    public async Task TheDraftTellsThePickedPlayerAndTheCaptainOnTheClock()
    {
        using var hub = new TestHub();

        var championship = await CreateAsync(hub, "Draft cup", Knockout());

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipService>().UpdateAsync(championship.Id, new ChampionshipSettingsForm { Name = "Draft cup", PlayersPerGame = 2, RosterSize = 2, DraftMethod = ChampionshipDraftMethod.Linear }, Admin));
        await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().CreateTeamsAsync(championship.Id, 2, "Team", Admin));

        var teams = (await LoadAsync(hub, championship.Id)).Teams.OrderBy(team => team.Seed).ToList();

        for (var index = 0; index < teams.Count; index++)
        {
            var teamId = teams[index].Id;
            var captain = FirstPlayer + index;

            await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().SetLeadersAsync(teamId, captain, null, Admin));
        }

        Assert.IsTrue((await LatestAsync(hub, FirstPlayer, false)).Any(item => item.Text.StartsWith("You are the captain of {0}")));

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().AddParticipantsAsync(championship.Id, [FirstPlayer + 10, FirstPlayer + 11], Admin));
        await hub.RunAsync(services => services.GetRequiredService<IChampionshipService>().SetStatusAsync(championship.Id, ChampionshipStatus.TeamFormation, null, Admin));
        await hub.RunAsync(services => services.GetRequiredService<IChampionshipDraftService>().DrawOrderAsync(championship.Id, Admin));

        var order = (await LoadAsync(hub, championship.Id)).Teams.OrderBy(team => team.DraftPosition).ToList();
        var first = order[0].CaptainCommunityId!.Value;
        var second = order[1].CaptainCommunityId!.Value;

        Assert.IsTrue((await LatestAsync(hub, first, false)).Any(item => item.Text.StartsWith("It is the turn of {0}")));
        Assert.IsFalse((await LatestAsync(hub, second, false)).Any(item => item.Text.StartsWith("It is the turn of {0}")));

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipDraftService>().CaptainPickAsync(championship.Id, FirstPlayer + 10, first));

        var picked = (await LatestAsync(hub, FirstPlayer + 10, false)).Single(item => item.Text == "{0} chose you in the draft of {1}.");

        Assert.AreEqual(order[0].Name, picked.Values[0]);
        Assert.IsTrue((await LatestAsync(hub, second, false)).Any(item => item.Text.StartsWith("It is the turn of {0}")));
    }

    [TestMethod]
    public async Task ANewAdministratorIsTold()
    {
        using var hub = new TestHub();

        await hub.RunAsync(services => services.GetRequiredService<IAdministratorService>().AddAsync(Player, "Someone"));

        var notification = (await LatestAsync(hub, Player, false)).Single();

        Assert.AreEqual(NotificationTopic.Administration, notification.Topic);
        Assert.AreEqual("/admin", notification.Url);
    }

    private static NotificationMessage Message(string text)
    {
        return new NotificationMessage(NotificationTopic.Championships, text, null);
    }

    private static Task SendAsync(TestHub hub, Action<INotificationService> send)
    {
        return hub.RunAsync(async services =>
        {
            send(services.GetRequiredService<INotificationService>());
            await services.GetRequiredService<HubDbContext>().SaveChangesAsync();
        });
    }

    private static Task<List<NotificationItem>> LatestAsync(TestHub hub, long communityId, bool administrator)
    {
        return hub.RunAsync(services => services.GetRequiredService<INotificationService>().LatestAsync(communityId, administrator, 50));
    }

    private static Task<int> UnreadAsync(TestHub hub, long communityId, bool administrator)
    {
        return hub.RunAsync(services => services.GetRequiredService<INotificationService>().UnreadAsync(communityId, administrator));
    }
}
