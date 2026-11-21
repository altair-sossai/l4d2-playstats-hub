using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Players;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Core;

[TestClass]
public class PlayerRegistryTests
{
    private const long Player = 76561198000000501;
    private const long Connected = 76561198000000502;
    private static readonly DateTime Night = new(2026, 11, 14, 21, 0, 0);

    [TestMethod]
    public void TheDisplayNameIsTheForcedNameThenTheMostUsedThenTheSteamNameThenTheLastSeenThenTheSteamId()
    {
        var profile = new PlayerProfile { CommunityId = Player, ForcedName = "Forced", MostUsedName = "Most used", SteamName = "Steam", Name = "Last seen" };

        Assert.AreEqual("Forced", PlayerDisplayNames.Resolve(profile));

        profile.ForcedName = null;
        Assert.AreEqual("Most used", PlayerDisplayNames.Resolve(profile));

        profile.MostUsedName = " ";
        Assert.AreEqual("Steam", PlayerDisplayNames.Resolve(profile));

        profile.SteamName = null;
        Assert.AreEqual("Last seen", PlayerDisplayNames.Resolve(profile));

        profile.Name = null;
        Assert.AreEqual(Player.ToString(), PlayerDisplayNames.Resolve(profile));
        Assert.IsNull(PlayerDisplayNames.Known(Player, PlayerDisplayNames.Resolve(profile)));
    }

    [TestMethod]
    public void TheMostUsedNameIgnoresTheCaseAndKeepsTheFirstOneUsedOnATie()
    {
        PlayerNameUsage[] usages =
        [
            new(Player, "Ellis", 2, 10),
            new(Player, "ELLIS", 2, 30),
            new(Player, "Nick", 3, 5),
            new(Player, "Coach", 4, 40),
            new(Player, " ", 9, 1)
        ];

        Assert.AreEqual("Ellis", MostUsedNames.Pick(usages));
        Assert.AreEqual("Nick", MostUsedNames.Pick([new PlayerNameUsage(Player, "Rochelle", 3, 8), new PlayerNameUsage(Player, "Nick", 3, 5)]));
        Assert.IsNull(MostUsedNames.Pick([]));
    }

    [TestMethod]
    public async Task UploadingAMatchRecalculatesTheMostUsedNameOfItsPlayers()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await UploadMatchAsync(hub, server, Night.AddDays(-3), "Alpha");

        var afterFirst = await ProfileAsync(hub, StatisticsBuilder.PlayerA1);

        await UploadMatchAsync(hub, server, Night.AddDays(-2), "Beta");
        await UploadMatchAsync(hub, server, Night.AddDays(-1), "Beta");

        var afterThird = await ProfileAsync(hub, StatisticsBuilder.PlayerA1);
        var ranking = await hub.RunAsync(services => services.GetRequiredService<IRankingService>().AllTimeAsync());
        var store = await hub.RunAsync(services => Task.FromResult(services.GetRequiredService<IDisplayNameStore>().Find(StatisticsBuilder.PlayerA1)));

        Assert.AreEqual("Alpha", afterFirst.MostUsedName);
        Assert.AreEqual("Alpha", afterFirst.DisplayName);
        Assert.AreEqual("Beta", afterThird.MostUsedName);
        Assert.AreEqual("Beta", afterThird.DisplayName);
        Assert.AreEqual("Beta", afterThird.Name);
        Assert.AreEqual("Beta", ranking.Single(player => player.CommunityId == StatisticsBuilder.PlayerA1).MainName);
        Assert.AreEqual("Beta", store);
    }

    [TestMethod]
    public async Task ThePlayerSearchFindsAPlayerByANameUsedInAnOldMatchAndShowsTheDisplayName()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await UploadMatchAsync(hub, server, Night.AddDays(-3), "Alpha");
        await UploadMatchAsync(hub, server, Night.AddDays(-2), "Beta");
        await UploadMatchAsync(hub, server, Night.AddDays(-1), "Beta");

        var found = await hub.RunAsync(services => services.GetRequiredService<IPlayerDirectoryService>().SearchAsync("alph", 1));
        var all = await hub.RunAsync(services => services.GetRequiredService<IPlayerDirectoryService>().SearchAsync(null, 1));
        var player = found.Items.Single();

        Assert.AreEqual(1, found.Total);
        Assert.AreEqual(StatisticsBuilder.PlayerA1, player.CommunityId);
        Assert.AreEqual("Beta", player.Name);
        CollectionAssert.AreEqual(new[] { "Beta", "Alpha" }, player.Names);
        Assert.IsTrue(player.Ranked);
        Assert.AreEqual(await hub.RunAsync(services => services.GetRequiredService<HubDbContext>().PlayerProfiles.CountAsync()), all.Total);
    }

    [TestMethod]
    public async Task AForcedNameWinsOverTheMostUsedNameAndGoesAwayWhenRemoved()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await UploadMatchAsync(hub, server, Night.AddDays(-1), "Alpha");
        await hub.RunAsync(services => services.GetRequiredService<IForcedNameService>().SaveAsync(StatisticsBuilder.PlayerA1, "The Boss", null));

        var forced = await ProfileAsync(hub, StatisticsBuilder.PlayerA1);
        var ranked = await hub.RunAsync(services => services.GetRequiredService<IRankingService>().AllTimeAsync());

        await hub.RunAsync(services => services.GetRequiredService<IForcedNameService>().DeleteAsync(StatisticsBuilder.PlayerA1));

        var removed = await ProfileAsync(hub, StatisticsBuilder.PlayerA1);
        var api = await hub.RunAsync(services => Task.FromResult(services.GetRequiredService<IForcedNameStore>().All.Count));

        Assert.AreEqual("The Boss", forced.ForcedName);
        Assert.AreEqual("The Boss", forced.DisplayName);
        Assert.AreEqual("The Boss", ranked.Single(player => player.CommunityId == StatisticsBuilder.PlayerA1).MainName);
        Assert.IsNull(removed.ForcedName);
        Assert.AreEqual("Alpha", removed.DisplayName);
        Assert.AreEqual(0, api);
    }

    [TestMethod]
    public async Task TheBackfillCreatesTheMissingProfilesAndRunningItAgainChangesNothing()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await UploadMatchAsync(hub, server, Night.AddDays(-1), "Alpha");
        await hub.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();

            await dbContext.PlayerProfiles.Where(profile => profile.CommunityId == StatisticsBuilder.PlayerA1).ExecuteDeleteAsync();
            await dbContext.PlayerProfiles.ExecuteUpdateAsync(setters => setters.SetProperty(profile => profile.DisplayName, (string?)null).SetProperty(profile => profile.MostUsedName, (string?)null));

            dbContext.Set<PlayerConnection>().Add(new PlayerConnection { IpAddress = "200.100.50.1", CommunityId = Connected, Name = "OnlyConnected", FirstSeenAtUtc = Night.AddDays(-5), LastSeenAtUtc = Night.AddDays(-4), Count = 2 });

            await dbContext.SaveChangesAsync();
        });

        var first = await hub.RunAsync(services => services.GetRequiredService<IPlayerRegistryService>().BackfillAsync());
        var second = await hub.RunAsync(services => services.GetRequiredService<IPlayerRegistryService>().BackfillAsync());
        var player = await ProfileAsync(hub, StatisticsBuilder.PlayerA1);
        var connected = await ProfileAsync(hub, Connected);
        var unnamed = await hub.RunAsync(services => services.GetRequiredService<HubDbContext>().PlayerProfiles.CountAsync(profile => profile.DisplayName == null));

        Assert.AreEqual(2, first.Created);
        Assert.AreEqual(first.Profiles, first.Updated);
        Assert.AreEqual(0, second.Created);
        Assert.AreEqual(0, second.Updated);
        Assert.AreEqual("Alpha", player.DisplayName);
        Assert.AreEqual("Alpha", player.Name);
        Assert.AreEqual("OnlyConnected", connected.DisplayName);
        Assert.AreEqual(Night.AddDays(-5), connected.FirstSeenAtUtc);
        Assert.AreEqual(Night.AddDays(-4), connected.LastSeenAtUtc);
        Assert.AreEqual(0, unnamed);
    }

    private static async Task UploadMatchAsync(TestHub hub, L4D2PlayStats.Hub.Core.Modules.Servers.GameServer server, DateTime start, string name)
    {
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", start, 1, 300, 200).WithName(StatisticsBuilder.PlayerA1, name));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", start.AddMinutes(15), 2, 600, 500).WithName(StatisticsBuilder.PlayerA1, name));
    }

    private static Task<PlayerProfile> ProfileAsync(TestHub hub, long communityId)
    {
        return hub.RunAsync(services => services.GetRequiredService<HubDbContext>().PlayerProfiles.AsNoTracking().SingleAsync(profile => profile.CommunityId == communityId));
    }
}
