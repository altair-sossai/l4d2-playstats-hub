using System.Net;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Community;
using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Support.Paging;
using L4D2PlayStats.Hub.Tests.Web.Live;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Models.Players;
using Microsoft.Extensions.DependencyInjection;
using WebConsts = L4D2PlayStats.Hub.Web.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public partial class PaginationTests
{
    [TestMethod]
    public void APageAlreadyCutByTheDatabaseKeepsItsItemsAndClampsThePage()
    {
        var page = new PagedListModel<int>([41, 42], 9, 20, 42);
        var empty = new PagedListModel<int>([], 3, 20, 0);

        Assert.AreEqual(3, page.Page);
        Assert.AreEqual(3, page.TotalPages);
        Assert.AreEqual(41, page.FirstItem);
        Assert.AreEqual(42, page.LastItem);
        CollectionAssert.AreEqual(new[] { 41, 42 }, page.Items.ToList());
        Assert.AreEqual(1, empty.Page);
        Assert.AreEqual(0, empty.FirstItem);
        Assert.AreEqual(40, Paging.Skip(3, 20));
        Assert.AreEqual(1, Paging.Page(-5, 10, 20));
    }

    [TestMethod]
    public void ThePlayersPageLinksKeepTheOrderAndThePeriod()
    {
        Assert.AreEqual("/players?orderBy=Wins&asc=True&season=3&page=2", PlayerUrls.List(PlayerResultProperty.Wins, true, "season=3", 2));
        Assert.AreEqual("/players?orderBy=Experience&asc=False&page=4", PlayerUrls.List(PlayerResultProperty.Experience, false, null, 4));
    }

    [TestMethod]
    public async Task ThePlayersListShowsOneHundredPlayersPerPageAndThePositionsFollowThePage()
    {
        using var factory = new PagingWebFactory(150);
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var first = WebUtility.HtmlDecode(await client.GetStringAsync("/players?orderBy=Wins&asc=False&period=all-time"));
        var second = WebUtility.HtmlDecode(await client.GetStringAsync("/players?orderBy=Wins&asc=False&period=all-time&page=2"));
        var ascending = WebUtility.HtmlDecode(await client.GetStringAsync("/players?orderBy=Wins&asc=True&page=2"));
        var beyond = WebUtility.HtmlDecode(await client.GetStringAsync("/players?orderBy=Wins&asc=False&period=all-time&page=99"));

        Assert.AreEqual(WebConsts.PlayersPageSize, PlayerPosition().Count(first));
        Assert.Contains("Showing 1–100 of 150", first);
        Assert.Contains(">#1<", first);
        Assert.Contains(">#100<", first);
        Assert.DoesNotContain(">#101<", first);
        Assert.Contains($">{PagingSamples.Name(0)}<", first);
        Assert.Contains("/players?orderBy=Wins&asc=False&period=all-time&page=2", first);
        Assert.Contains("150 players", first);
        Assert.AreEqual(50, PlayerPosition().Count(second));
        Assert.Contains("Showing 101–150 of 150", second);
        Assert.Contains(">#101<", second);
        Assert.Contains(">#150<", second);
        Assert.Contains($">{PagingSamples.Name(149)}<", second);
        Assert.DoesNotContain($">{PagingSamples.Name(0)}<", second);
        Assert.Contains("/players?orderBy=Wins&asc=False&period=all-time&page=1", second);
        Assert.Contains("Showing 101–150 of 150", ascending);
        Assert.Contains(">#50<", ascending);
        Assert.Contains(">#1<", ascending);
        Assert.DoesNotContain(">#51<", ascending);
        Assert.Contains($">{PagingSamples.Name(0)}<", ascending);
        Assert.Contains("/players?orderBy=Wins&asc=True&page=1", ascending);
        Assert.Contains("Showing 101–150 of 150", beyond);
    }

    [TestMethod]
    public async Task ASmallPlayersListHasNoPageLinks()
    {
        using var factory = new PagingWebFactory(8);
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var page = WebUtility.HtmlDecode(await client.GetStringAsync("/players?orderBy=Wins&asc=False"));

        Assert.AreEqual(8, PlayerPosition().Count(page));
        Assert.DoesNotContain("class=\"admin-pagination\"", page);
    }

    [TestMethod]
    public async Task TheCommunityRankingShowsOneHundredPlayersPerPage()
    {
        using var factory = new PagingWebFactory(150);
        using var client = factory.Client();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { Ranking = true, CommunityRanking = true });

        var first = WebUtility.HtmlDecode(await client.GetStringAsync("/community"));
        var second = WebUtility.HtmlDecode(await client.GetStringAsync("/community?page=2"));

        Assert.Contains("Showing 1–100 of 150", first);
        Assert.Contains("data-position=\"100\"", first);
        Assert.DoesNotContain("data-position=\"101\"", first);
        Assert.Contains("/community?page=2", first);
        Assert.Contains("Showing 101–150 of 150", second);
        Assert.Contains("data-position=\"101\"", second);
        Assert.Contains("data-position=\"150\"", second);
        Assert.DoesNotContain("data-position=\"100\"", second);
        Assert.Contains($"/player/{PagingSamples.CommunityId(149)}", second);
        Assert.Contains("/community?page=1", second);
    }

    [TestMethod]
    public async Task TheVotersOfTheCommunityRankingAreListedTwentyPerPage()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { Ranking = true, CommunityRanking = true });
        await factory.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();
            var now = DateTime.UtcNow;

            for (var index = 0; index < 25; index++)
                dbContext.Set<CommunityVote>().Add(new CommunityVote
                {
                    VoterId = PagingSamples.CommunityId(index),
                    LeftPlayerId = PagingSamples.CommunityId(100),
                    RightPlayerId = PagingSamples.CommunityId(101),
                    Outcome = CommunityVoteOutcome.Left,
                    CreatedAtUtc = now,
                    SiteDay = now.Date
                });

            await dbContext.SaveChangesAsync();
        });

        var first = WebUtility.HtmlDecode(await client.GetStringAsync("/admin/community"));
        var second = WebUtility.HtmlDecode(await client.GetStringAsync("/admin/community?page=2"));

        Assert.Contains("Showing 1–20 of 25", first);
        Assert.Contains(Voter(0), first);
        Assert.Contains(Voter(19), first);
        Assert.DoesNotContain(Voter(20), first);
        Assert.Contains("/admin/community?page=2#ca-voters", first);
        Assert.Contains("id=\"ca-voters\"", first);
        Assert.Contains("Showing 21–25 of 25", second);
        Assert.Contains(Voter(20), second);
        Assert.Contains(Voter(24), second);
        Assert.DoesNotContain(Voter(19), second);
    }

    [TestMethod]
    public async Task TheAuditLogOfTheRemoteCommandsIsListedFiftyPerPageWithTheTotalsOfEveryCommand()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();

        var credentials = await factory.CreateServerAsync();

        await factory.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();
            var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            for (var index = 0; index < 60; index++)
                dbContext.Set<RemoteCommandLog>().Add(new RemoteCommandLog
                {
                    GameServerId = credentials.Server.Id,
                    GameServerName = credentials.Server.Name,
                    AdministratorCommunityId = HubWebFactory.Administrator,
                    Command = $"sm_cmd{index:00}",
                    CreatedAtUtc = start.AddMinutes(index),
                    DeliveredAtUtc = index % 3 == 0 ? start.AddMinutes(index) : null
                });

            await dbContext.SaveChangesAsync();
        });

        var first = WebUtility.HtmlDecode(await client.GetStringAsync("/admin/remote-commands"));
        var second = WebUtility.HtmlDecode(await client.GetStringAsync("/admin/remote-commands?page=2"));

        Assert.Contains("Showing 1–50 of 60", first);
        Assert.Contains("<code>sm_cmd59</code>", first);
        Assert.Contains("<code>sm_cmd10</code>", first);
        Assert.DoesNotContain("<code>sm_cmd09</code>", first);
        Assert.Contains("<strong>60</strong>", first);
        Assert.Contains("<strong>20</strong>", first);
        Assert.Contains("/admin/remote-commands?page=2", first);
        Assert.Contains("Showing 51–60 of 60", second);
        Assert.Contains("<code>sm_cmd09</code>", second);
        Assert.Contains("<code>sm_cmd00</code>", second);
        Assert.DoesNotContain("<code>sm_cmd10</code>", second);
    }

    private static string Voter(int index)
    {
        return $"name=\"communityId\" value=\"{PagingSamples.CommunityId(index)}\"";
    }

    [GeneratedRegex("<td class=\"text-center vertical-align-middle slim\">#")]
    private static partial Regex PlayerPosition();
}
