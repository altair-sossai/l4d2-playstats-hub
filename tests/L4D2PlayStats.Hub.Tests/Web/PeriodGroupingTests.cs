using System.Net;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class PeriodGroupingTests
{
    [TestMethod]
    public async Task ThePlayerPagesOfferTheSeasonsOfThisYearAndOneEntryPerYear()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        var (now, before, oldSeason) = await TwoYearsAsync(factory);

        var current = await client.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}");
        var oldYear = await client.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}?year={before.Year}");
        var thisYear = await client.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}?year={now.Year}");
        var list = await client.GetStringAsync($"/players?year={before.Year}");
        var oldSeasonPage = await client.GetAsync($"/player/{StatisticsBuilder.PlayerA1}?season={oldSeason}");
        var unknown = await client.GetAsync($"/player/{StatisticsBuilder.PlayerA1}?year=1900");
        var unknownList = await client.GetAsync("/players?year=1900");

        Assert.Contains($"/player/{StatisticsBuilder.PlayerA1}?year={now.Year}", current);
        Assert.Contains($"/player/{StatisticsBuilder.PlayerA1}?year={before.Year}", current);
        Assert.DoesNotContain($"?season={oldSeason}\"", current);
        Assert.Contains("Current season", current);

        Assert.Contains("The Parish", oldYear);
        Assert.DoesNotContain("Dark Carnival", oldYear);
        Assert.Contains($"Year of {before.Year}", oldYear);
        Assert.DoesNotContain("onclick=\"editPunishment(", oldYear);

        Assert.Contains("Dark Carnival", thisYear);
        Assert.DoesNotContain("The Parish", thisYear);

        Assert.Contains($"/player/{StatisticsBuilder.PlayerA1}?year={before.Year}", list);
        Assert.Contains($"asc=False&amp;year={before.Year}", list);

        Assert.AreEqual(HttpStatusCode.OK, oldSeasonPage.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, unknownList.StatusCode);
    }

    [TestMethod]
    public async Task TheMatchesListFiltersByYear()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        var (now, before, oldSeason) = await TwoYearsAsync(factory);

        var all = await client.GetStringAsync("/matches");
        var oldYear = await client.GetStringAsync($"/matches?year={before.Year}");
        var thisYear = await client.GetStringAsync($"/matches?year={now.Year}");

        Assert.Contains($"/matches?year={now.Year}\"", all);
        Assert.Contains($"/matches?year={before.Year}\"", all);
        Assert.DoesNotContain($"/matches?season={oldSeason}\"", all);
        Assert.Contains("The Parish", all);
        Assert.Contains("Dark Carnival", all);

        Assert.Contains("The Parish", oldYear);
        Assert.DoesNotContain("Dark Carnival", oldYear);
        Assert.Contains("Dark Carnival", thisYear);
        Assert.DoesNotContain("The Parish", thisYear);
    }

    [TestMethod]
    public async Task TheMatchesListAndTheInGamePagesNameTheSeasonLikeThePeriodSelector()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        var (now, _, _) = await TwoYearsAsync(factory);

        var current = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Seasons.AsNoTracking().Where(season => season.StartsAt <= now && season.EndsAt > now).SingleAsync());
        var last = current.EndsAt.AddDays(-1);
        var name = $"From {current.StartsAt:MM}/{current.StartsAt:yyyy} to {last:MM}/{last:yyyy}";

        var matches = await client.GetStringAsync("/matches");
        var inGame = await client.GetStringAsync("/ingame");
        var player = await client.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}");

        Assert.Contains($"\">{name}</option>", matches);
        Assert.Contains($"<div class=\"season\">{name}</div>", inGame);
        Assert.Contains($"{name} (Current season)", player);
        Assert.DoesNotContain(" – ", matches);
        Assert.DoesNotContain(" – ", inGame);
    }

    [TestMethod]
    public async Task TheHistoryListsOnlyTheSeasonsOfThisYearButStillOpensAnOlderOne()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        var (_, before, oldSeason) = await TwoYearsAsync(factory);

        var oldStart = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Seasons.Where(season => season.Id == oldSeason).Select(season => season.StartsAt).SingleAsync());
        var oldId = $"{oldStart:yyyy-MM}";

        var history = await client.GetStringAsync("/history");
        var old = await client.GetStringAsync($"/history/{oldId}");

        Assert.Contains($"/history/{before.Year}\"", history);
        Assert.Contains("/history/all-time\"", history);
        Assert.DoesNotContain($"/history/{oldId}\"", history);
        Assert.Contains("table-ranking", old);
        Assert.DoesNotContain("History not found", old);
    }

    [TestMethod]
    public async Task TheMenuCarriesWhatItNeedsToStayOnOneLine()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var body = await client.GetStringAsync("/");

        Assert.Contains("id=\"nav-main\"", body);
        Assert.Contains("id=\"nav-more\"", body);
        Assert.Contains("id=\"nav-more-menu\"", body);
        Assert.Contains("window.fitNavbar = fit", body);
        Assert.Contains("class=\"nav-fit active\"", body);
    }

    [TestMethod]
    public async Task ThePlayerPagesOfferAllTime()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await TwoYearsAsync(factory);

        var current = await client.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}");
        var allTime = await client.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}?period=all-time");
        var list = await client.GetStringAsync("/players?period=all-time");

        Assert.Contains($"/player/{StatisticsBuilder.PlayerA1}?period=all-time", current);
        Assert.Contains("The Parish", allTime);
        Assert.Contains("Dark Carnival", allTime);
        Assert.DoesNotContain("onclick=\"editPunishment(", allTime);
        Assert.Contains($"/player/{StatisticsBuilder.PlayerA1}?period=all-time", list);
    }

    [TestMethod]
    public async Task TheHomeRankingLetsTheVisitorChooseThePeriod()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        var (now, before, _) = await TwoYearsAsync(factory);

        var current = await client.GetStringAsync("/");
        var oldYear = await client.GetStringAsync($"/?year={before.Year}");
        var allTime = await client.GetStringAsync("/?period=all-time");
        var unknown = await client.GetAsync("/?year=1900");

        Assert.Contains("Displayed period", current);
        Assert.Contains("Current season", current);
        Assert.Contains($"href=\"/?year={now.Year}\"", current);
        Assert.Contains($"href=\"/?year={before.Year}\"", current);
        Assert.Contains("href=\"/?period=all-time\"", current);
        Assert.Contains("/imgs/patents/", current);
        Assert.Contains("Next reset in", current);

        Assert.Contains($"Year of {before.Year}", oldYear);
        Assert.Contains("table-ranking", oldYear);
        Assert.DoesNotContain("/imgs/patents/", oldYear);
        Assert.DoesNotContain("Next reset in", oldYear);
        Assert.Contains($"/player/{StatisticsBuilder.PlayerA1}?year={before.Year}", oldYear);
        Assert.Contains("href=\"/\"", oldYear);

        Assert.Contains("table-ranking", allTime);
        Assert.Contains($"/player/{StatisticsBuilder.PlayerA1}?period=all-time", allTime);
        Assert.AreEqual(HttpStatusCode.NotFound, unknown.StatusCode);
    }

    [TestMethod]
    public async Task TheCommunityRankingSitsNextToTheRankingInTheMenu()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        await factory.RunAsync(async services =>
        {
            var settings = services.GetRequiredService<ISettingsService>();
            var modules = await settings.GetAsync<ModuleSettings>();

            modules.CommunityRanking = true;

            await settings.SaveAsync(modules);

            return true;
        });

        var body = await client.GetStringAsync("/");
        var ranking = body.IndexOf("<a href=\"/\">Ranking</a>", StringComparison.Ordinal);
        var community = body.IndexOf("<a href=\"/community\">", StringComparison.Ordinal);
        var matches = body.IndexOf("<a href=\"/matches\">", StringComparison.Ordinal);

        Assert.IsGreaterThan(0, ranking);
        Assert.IsGreaterThan(ranking, community);
        Assert.IsGreaterThan(community, matches);
    }

    private static async Task<(DateTime Now, DateTime Before, int OldSeason)> TwoYearsAsync(HubWebFactory factory)
    {
        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        var now = DateTime.Now.Date.AddHours(-3);
        var before = now.AddMonths(-14);

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c5m1_waterfront", before, 1, 300, 200));
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c5m2_park", before.AddMinutes(15), 2, 600, 500));
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", now, 1, 300, 200));
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", now.AddMinutes(15), 2, 600, 500));

        var oldSeason = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Matches.Where(match => match.Campaign == "The Parish").Select(match => match.SeasonId).SingleAsync());

        return (now, before, oldSeason);
    }
}
