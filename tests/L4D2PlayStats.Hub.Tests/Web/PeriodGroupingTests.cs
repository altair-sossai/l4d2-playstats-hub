using System.Net;
using L4D2PlayStats.Hub.Core.Data;
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
