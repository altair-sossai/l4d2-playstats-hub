using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Players;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Core;

[TestClass]
public class PlayerSearchTests
{
    private const long Player = 76561197960287930;
    private const long Other = 76561198000000001;

    [TestMethod]
    [DataRow("76561197960287930")]
    [DataRow("STEAM_0:0:11101")]
    [DataRow("STEAM_1:0:11101")]
    [DataRow("[U:1:22202]")]
    [DataRow("U:1:22202")]
    [DataRow("https://steamcommunity.com/profiles/76561197960287930")]
    [DataRow("  steamcommunity.com/profiles/76561197960287930/  ")]
    public void EverySteamIdFormatFindsThePlayer(string term)
    {
        var search = PlayerSearch.Of(term);

        Assert.AreEqual(Player, search.CommunityId);
        Assert.IsTrue(search.Matches(Player, "Someone"));
        Assert.IsFalse(search.Matches(Other, "Someone"));
    }

    [TestMethod]
    [DataRow("ell")]
    [DataRow("ELL")]
    [DataRow("  Ellis ")]
    public void ThePlayerIsFoundByAnyOfItsNamesIgnoringTheCase(string term)
    {
        var search = PlayerSearch.Of(term);

        Assert.IsNull(search.CommunityId);
        Assert.IsTrue(search.Matches(Other, "Nick", "Ellis"));
        Assert.IsFalse(search.Matches(Other, "Nick", null));
    }

    [TestMethod]
    public void PartOfTheSteamId64FindsThePlayer()
    {
        Assert.IsTrue(PlayerSearch.Of("1198000").Matches(Other));
        Assert.IsFalse(PlayerSearch.Of("1198000").Matches(Player));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void AnEmptySearchFindsEveryone(string? term)
    {
        var search = PlayerSearch.Of(term);

        Assert.IsTrue(search.IsEmpty);
        Assert.IsNull(search.Term);
        Assert.IsTrue(search.Matches(Player));
    }

    [TestMethod]
    [DataRow("STEAM_1:0:11101", Player)]
    [DataRow("[U:1:22202]", Player)]
    [DataRow("ELLIS", Player)]
    [DataRow("coach", Other)]
    [DataRow("98000000", Other)]
    public async Task TheDatabaseAppliesTheSameSearch(string term, long expected)
    {
        using var hub = new TestHub();

        await hub.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();

            dbContext.Add(new PlayerProfile { CommunityId = Player, ForcedName = "Ellis", FirstSeenAtUtc = hub.Clock.UtcNow, LastSeenAtUtc = hub.Clock.UtcNow });
            dbContext.Add(new PlayerProfile { CommunityId = Other, ForcedName = "Coach", FirstSeenAtUtc = hub.Clock.UtcNow, LastSeenAtUtc = hub.Clock.UtcNow });

            await dbContext.SaveChangesAsync();
        });

        var found = await hub.RunAsync(services => PlayerSearch.Of(term)
            .Apply(services.GetRequiredService<HubDbContext>().PlayerProfiles.AsNoTracking(), entry => entry.CommunityId, entry => entry.ForcedName)
            .Select(entry => entry.CommunityId)
            .ToListAsync());

        CollectionAssert.AreEqual(new[] { expected }, found);
    }

    [TestMethod]
    public async Task AnEmptySearchKeepsTheWholeTable()
    {
        using var hub = new TestHub();

        await hub.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();

            dbContext.Add(new PlayerProfile { CommunityId = Player, ForcedName = "Ellis", FirstSeenAtUtc = hub.Clock.UtcNow, LastSeenAtUtc = hub.Clock.UtcNow });
            dbContext.Add(new PlayerProfile { CommunityId = Other, ForcedName = "Coach", FirstSeenAtUtc = hub.Clock.UtcNow, LastSeenAtUtc = hub.Clock.UtcNow });

            await dbContext.SaveChangesAsync();
        });

        var count = await hub.RunAsync(services => PlayerSearch.Of(" ")
            .Apply(services.GetRequiredService<HubDbContext>().PlayerProfiles.AsNoTracking(), entry => entry.CommunityId, entry => entry.ForcedName)
            .CountAsync());

        Assert.AreEqual(2, count);
    }
}
