using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Models;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Punishments.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Support.Skins;
using Microsoft.Extensions.DependencyInjection;
using static L4D2PlayStats.Hub.Tests.Core.Championships.ChampionshipFixture;

namespace L4D2PlayStats.Hub.Tests.Core.Notifications;

[TestClass]
public class NotificationEventTests
{
    private const long Player = 76561198100000500;
    private static readonly DateTime Friday = new(2026, 11, 20, 22, 0, 0, DateTimeKind.Utc);

    [TestMethod]
    public async Task TheStatusOfAChampionshipAndTheParticipantsRemovedAreTold()
    {
        using var hub = new TestHub();

        var championship = await CreateAsync(hub, "Status cup", Knockout());

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().AddParticipantsAsync(championship.Id, [Player, Player + 1], Admin));
        Assert.AreEqual("You are a participant of {0}.", (await TextsAsync(hub, Player)).Single());

        await StatusAsync(hub, championship.Id, ChampionshipStatus.TeamFormation);
        Assert.AreEqual("A new championship was published: {0}.", (await TextsAsync(hub, Player + 50)).Single());

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipService>().UpdateAsync(championship.Id, new ChampionshipSettingsForm { Name = "Status cup", JoinRequests = true }, Admin));
        await StatusAsync(hub, championship.Id, ChampionshipStatus.Participants);
        Assert.AreEqual("The registrations of {0} are open.", (await TextsAsync(hub, Player + 50))[0]);

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().RemoveParticipantAsync(championship.Id, Player + 1, Admin));
        Assert.AreEqual("You are no longer a participant of {0}.", (await TextsAsync(hub, Player + 1))[0]);

        await StatusAsync(hub, championship.Id, ChampionshipStatus.Running);
        Assert.AreEqual("{0} has started.", (await TextsAsync(hub, Player))[0]);

        await StatusAsync(hub, championship.Id, ChampionshipStatus.Cancelled);
        Assert.AreEqual("{0} was cancelled.", (await TextsAsync(hub, Player))[0]);
        Assert.IsFalse((await TextsAsync(hub, Player + 1)).Contains("{0} was cancelled."));
        Assert.IsFalse((await TextsAsync(hub, Player + 50)).Contains("{0} has started."));
    }

    [TestMethod]
    public async Task ARefusedDateAndAWalkoverAreTold()
    {
        using var hub = new TestHub();

        var championship = await CreateAsync(hub, "Cup", Knockout());
        var teams = await TeamsAsync(hub, championship.Id, 2);
        var stageId = (await LoadAsync(hub, championship.Id)).Stages.Single().Id;

        await GenerateAsync(hub, stageId);
        Assert.IsTrue((await TextsAsync(hub, FirstPlayer + 3)).Contains("The games of {0} in {1} are ready."));

        var series = (await LoadAsync(hub, championship.Id)).Stages.Single().Series.Single();
        var proposal = await hub.RunAsync(services => services.GetRequiredService<IChampionshipScheduleService>().ProposeAsync(series.Id, Friday, FirstPlayer));

        Assert.AreEqual("{0} proposed a date for the series against your team in {1}.", (await TextsAsync(hub, FirstPlayer + 101))[0]);

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipScheduleService>().AnswerAsync(proposal.Id, false, FirstPlayer + 100));
        Assert.AreEqual("{0} refused the date your team proposed in {1}.", (await TextsAsync(hub, FirstPlayer + 1))[0]);
        Assert.IsFalse((await TextsAsync(hub, FirstPlayer + 2)).Contains("{0} refused the date your team proposed in {1}."));

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipSeriesService>().WalkoverAsync(series.Id, teams[1], Admin));

        var walkover = (await LatestAsync(hub, FirstPlayer + 2)).First();

        Assert.AreEqual("{0} won the series against {1} by walkover in {2}.", walkover.Text);
        Assert.AreEqual("Team B", walkover.Values[0]);
        Assert.IsTrue((await TextsAsync(hub, FirstPlayer + 103)).Contains("{0} won the series against {1} by walkover in {2}."));
    }

    [TestMethod]
    public async Task PunishmentsForcedNamesAndCommunityBlocksTellThePlayer()
    {
        using var hub = new TestHub();

        var season = await hub.RunAsync(services => services.GetRequiredService<ISeasonService>().GetCurrentAsync());

        await hub.RunAsync(services => services.GetRequiredService<IPunishmentService>().AddAsync(season.Id, Player, 150, "Leaving", Admin));

        var punishment = (await LatestAsync(hub, Player)).Single();

        Assert.AreEqual("You lost {0} experience points in this season.", punishment.Text);
        CollectionAssert.AreEqual(new[] { "150" }, punishment.Values);
        Assert.AreEqual($"/player/{Player}", punishment.Url);

        await hub.RunAsync(services => services.GetRequiredService<IPunishmentService>().RemoveAsync(season.Id, Player));
        await hub.RunAsync(services => services.GetRequiredService<IForcedNameService>().SaveAsync(Player, "The Boss", Admin));
        await hub.RunAsync(services => services.GetRequiredService<IForcedNameService>().DeleteAsync(Player));
        await hub.RunAsync(services => services.GetRequiredService<ICommunityModerationService>().BlockAsync(Player, Admin));
        await hub.RunAsync(services => services.GetRequiredService<ICommunityModerationService>().UnblockAsync(Player));

        CollectionAssert.AreEqual(new[]
        {
            "You can vote in the community ranking again.",
            "You can no longer vote in the community ranking.",
            "Your forced name was removed: the site shows the name you use in the game again.",
            "Your name on the servers and on the site is now {0}.",
            "Your punishments in this season were removed.",
            "You lost {0} experience points in this season."
        }, (await TextsAsync(hub, Player)).ToArray());
        Assert.IsEmpty(await TextsAsync(hub, Player + 1));
    }

    [TestMethod]
    public async Task AGiftAndARefundOfASkinTellThePlayer()
    {
        using var hub = new TestHub();

        await hub.ImportSkinsAsync();

        var player = await hub.PlayerWithStarsAsync();

        await hub.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.GoldMagnum));
        await hub.ShopAsync(shop => shop.RefundAsync(player, SkinSamples.GoldMagnum, Admin));
        await hub.ShopAsync(shop => shop.GiftAsync(player, SkinSamples.GalaxyPump, Admin, "Admin"));

        var notices = await LatestAsync(hub, player);

        Assert.AreEqual("You received the skin {0} as a gift. It is already equipped.", notices[0].Text);
        Assert.AreEqual("The skin {0} was refunded: its stars are back in your balance.", notices[1].Text);
        Assert.AreEqual("/skins", notices[0].Url);
        Assert.HasCount(2, notices);
    }

    [TestMethod]
    public async Task OnlyASeasonThatReplacesAnotherOneIsAnnounced()
    {
        using var hub = new TestHub();

        await hub.RunAsync(services => services.GetRequiredService<ISeasonService>().GetCurrentAsync());
        Assert.IsEmpty(await TextsAsync(hub, Player));

        hub.Clock.Advance(TimeSpan.FromDays(400));
        await hub.RunAsync(services => services.GetRequiredService<ISeasonService>().GetCurrentAsync());

        Assert.AreEqual("A new season started: the ranking starts again from zero.", (await TextsAsync(hub, Player)).Single());
    }

    private static Task StatusAsync(TestHub hub, int championshipId, ChampionshipStatus status)
    {
        return hub.RunAsync(services => services.GetRequiredService<IChampionshipService>().SetStatusAsync(championshipId, status, status == ChampionshipStatus.Cancelled ? "Not enough teams" : null, Admin));
    }

    private static Task<List<NotificationItem>> LatestAsync(TestHub hub, long communityId)
    {
        return hub.RunAsync(services => services.GetRequiredService<INotificationService>().LatestAsync(communityId, false, 50));
    }

    private static async Task<List<string>> TextsAsync(TestHub hub, long communityId)
    {
        return (await LatestAsync(hub, communityId)).Select(item => item.Text).ToList();
    }
}
