using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Helpers;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Match = L4D2PlayStats.Hub.Core.Modules.Matches.Match;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public partial class MatchPagesTests
{
    private const string TokenHeader = "RequestVerificationToken";

    [TestMethod]
    public async Task TheListShowsTheMatchCardOfTheOldSite()
    {
        using var factory = await FactoryWithMatchAsync();
        using var client = factory.Client();

        var match = await MatchAsync(factory);
        var body = await client.GetStringAsync("/matches");

        Assert.Contains("<h4 class=\"text-center\">Last matches</h4>", body);
        Assert.Contains("<div class=\"panel panel-primary\">", body);
        Assert.Contains("Dark Carnival", body);
        Assert.Contains($"<small style=\"font-style: italic\"> - {StatisticsBuilder.Sample().Statistics.GameRound!.ConfigurationName}</small>", body);
        Assert.Contains("label label-success", body);
        Assert.Contains("Team A won by 100 points", body);
        Assert.Contains("/imgs/campaigns/dark-carnival.jpg", body);
        Assert.Contains("data-lity", body);
        Assert.Contains($"href=\"/matches/{match.Id}\" class=\"btn btn-danger btn-xs btn-block\"", body);
        Assert.Contains("panel panel-success", body);
        Assert.Contains("panel panel-danger", body);
        Assert.Contains($"href=\"/player/{StatisticsBuilder.PlayerA1}\"", body);
        Assert.Contains($"src=\"/avatar/{StatisticsBuilder.PlayerA1}\"", body);
        Assert.Contains("/imgs/icons/star.png", body);
        Assert.Contains("/imgs/icons/star-blue.png", body);
        Assert.AreEqual(2, Regex.Count(body, "class=\"app-chart\""));
        Assert.Contains("&quot;data&quot;:[300,600]", body);
        Assert.Contains("&quot;data&quot;:[200,500]", body);
        Assert.Contains("&quot;data&quot;:[300,300]", body);
        Assert.Contains("&quot;labels&quot;:[&quot;c2m1_highway&quot;,&quot;c2m2_fairgrounds&quot;]", body);
        Assert.Contains("See the latest L4D2 PlayStats matches, including scores, campaigns, and players.", body);
        Assert.IsTrue(Regex.IsMatch(body, "<li class=\"nav-fit active\">\\s*<a href=\"/matches\">"));
    }

    [TestMethod]
    public async Task TheListOpensOnBootstrapThreeWithoutTheOldRedesign()
    {
        using var factory = await FactoryWithMatchAsync();
        using var client = factory.Client();

        var match = await MatchAsync(factory);
        var file = await LastFileAsync(factory);

        foreach (var page in new[] { "/matches", $"/matches/{match.Id}", $"/matches/{match.Id}/statistics/{file.Id}" })
        {
            var body = await client.GetStringAsync(page);

            Assert.Contains("/css/bootstrap.min.css", body, page);
            Assert.DoesNotContain("/lib/bootstrap", body, page);
            Assert.DoesNotContain("htmx", body, page);
        }
    }

    [TestMethod]
    public async Task TheSeasonAndServerFiltersNarrowTheList()
    {
        using var factory = await FactoryWithMatchAsync();
        using var client = factory.Client();

        var match = await MatchAsync(factory);
        var all = await client.GetStringAsync("/matches");
        var season = await client.GetStringAsync($"/matches?season={match.SeasonId}");
        var otherSeason = await client.GetStringAsync($"/matches?season={match.SeasonId + 1000}");
        var otherServer = await client.GetStringAsync($"/matches?server={match.GameServerId + 1000}");

        Assert.Contains("panel panel-primary", all);
        Assert.Contains("panel panel-primary", season);
        Assert.DoesNotContain("panel panel-primary", otherSeason);
        Assert.Contains("No match found", otherSeason);
        Assert.DoesNotContain("panel panel-primary", otherServer);
    }

    [TestMethod]
    public async Task TheServerNameOnlyAppearsWhenThereIsMoreThanOneServer()
    {
        using var factory = await FactoryWithMatchAsync("Alpha Arena");
        using var client = factory.Client();

        var single = await client.GetStringAsync("/matches");

        await factory.CreateServerAsync("Beta Arena");

        var several = await client.GetStringAsync("/matches");

        Assert.DoesNotContain("Alpha Arena", single);
        Assert.DoesNotContain("aria-label=\"Server\"", single);
        Assert.Contains(" - Alpha Arena</small>", several);
        Assert.Contains("aria-label=\"Server\"", several);
        Assert.Contains("Beta Arena</option>", several);
    }

    [TestMethod]
    public async Task TheDetailsPageShowsTheMapsAndBothTeamsAsSurvivorAndInfected()
    {
        using var factory = await FactoryWithMatchAsync();
        using var client = factory.Client();

        var match = await MatchAsync(factory);
        var files = await FilesAsync(factory);
        var body = await client.GetStringAsync($"/matches/{match.Id}");

        Assert.Contains("<h4 class=\"text-center\">Match details</h4>", body);
        Assert.Contains("<ol class=\"breadcrumb\">", body);
        Assert.Contains("<title>Match details - Dark Carnival | L4D2 PlayStats</title>", body);
        Assert.Contains("See details of the match on Dark Carnival, played on", body);
        Assert.DoesNotContain("btn btn-danger btn-xs btn-block", body);
        Assert.Contains("Map results", body);

        foreach (var file in files)
            Assert.Contains($"href=\"/matches/{match.Id}/statistics/{file.Id}\"", body);

        Assert.Contains("c2m1_highway", body);
        Assert.Contains("c2m2_fairgrounds", body);
        Assert.Contains("data-label=\"Team A score\"", body);
        Assert.Contains("data-label=\"Score difference\"", body);
        Assert.Contains("Team A - Survivor", body);
        Assert.Contains("Team B - Survivor", body);
        Assert.Contains("Team A - Infected", body);
        Assert.Contains("Team B - Infected", body);
        Assert.Contains("Statistics of team A playing as survivors", body);
        Assert.Contains("Statistics of team B playing as infected", body);

        foreach (var row in new[]
                 {
                     "SI MVP awards", "CI MVP awards", "FF LVP awards", "Deaths", "Times incapacitated", "SI damage taken", "Commons killed", "SI killed",
                     "SI - Damage dealt", "Damage to Tank", "Damage to Witch", "Rock hits taken", "Total skeets", "Melee skeets", "Total levels",
                     "Crowns (Witch one-shot)", "FF dealt (non-incap.)", "SI damage (excl. Tank)", "Damage as Tank (upright)", "Spit damage (upright)", "Total pounce damage"
                 })
            Assert.Contains($"<td>{row}", Regex.Replace(body, @"<td>\s+", "<td>"), row);

        Assert.AreEqual(4, Regex.Count(body, "<small>Team %</small>") / 4);
        Assert.Contains("class=\"text-left hide-mobile\"", body);
        Assert.Contains("glyphicon glyphicon-info-sign", body);
    }

    [TestMethod]
    public async Task OnlyAnAdministratorSeesTheButtonsToEditAScore()
    {
        using var factory = await FactoryWithMatchAsync();
        using var visitor = factory.Client();
        using var player = factory.Client(HubWebFactory.Player);
        using var administrator = factory.Client(HubWebFactory.Administrator);

        var match = await MatchAsync(factory);
        var page = $"/matches/{match.Id}";

        Assert.DoesNotContain("editScore(", await visitor.GetStringAsync(page));
        Assert.DoesNotContain("editScore(", await player.GetStringAsync(page));

        var body = await administrator.GetStringAsync(page);

        Assert.AreEqual(4, Regex.Count(body, "onclick=\"editScore\\("));
        Assert.Contains("/update-score", body);
        Assert.Contains("Invalid score range", body);
    }

    [TestMethod]
    public async Task TheStatisticsPageShowsBothRoundsOfTheMap()
    {
        using var factory = await FactoryWithMatchAsync();
        using var client = factory.Client();

        var match = await MatchAsync(factory);
        var file = await LastFileAsync(factory);
        var body = await client.GetStringAsync($"/matches/{match.Id}/statistics/{file.Id}");

        Assert.Contains("<title>c2m2_fairgrounds - Dark Carnival | L4D2 PlayStats</title>", body);
        Assert.Contains("See statistics for map c2m2_fairgrounds from the match on Dark Carnival, with a 600&#x2013;500 score.", body);
        Assert.Contains($"<a href=\"/matches/{match.Id}\">Dark Carnival</a>", body);
        Assert.AreEqual(1, Regex.Count(body, "<tr class=\"info\">"));
        Assert.Contains("href=\"#tab-round-half-1\"", body);
        Assert.Contains("href=\"#tab-round-half-2\"", body);
        Assert.Contains("1st round", body);
        Assert.Contains("2nd round", body);
        Assert.Contains("id=\"tab-round-half-1\" class=\"tab-pane fade in active match\"", body);
        Assert.Contains("/imgs/icons/lvp.png", body);
        Assert.Contains("Round start:", body);
        Assert.Contains("Round duration:", body);
        Assert.Contains("Commons killed:", body);
        Assert.Contains("Damage to SI:", body);
        Assert.Contains("SMG / Rifle - Shots:", body);
        Assert.Contains("SMG / Rifle - Hits (%):", body);
        Assert.Contains("SI damage (excl. Tank):", body);
        Assert.Contains("Spawns:", body);
        Assert.Contains("Connected Players", body);
        Assert.Contains("<th>Steam ID</th>", body);
        Assert.Contains("<th>Community ID</th>", body);
        Assert.Contains("icon-copy", body);
        Assert.Contains($"data-copy-text=\"{StatisticsBuilder.PlayerA1}\"", body);
        Assert.Contains($"href=\"/player/{StatisticsBuilder.PlayerA1}\"", body);
        Assert.DoesNotContain("editScore(", body);
    }

    [TestMethod]
    public async Task AMatchOrAFileThatDoesNotExistIsNotFound()
    {
        using var factory = await FactoryWithMatchAsync();
        using var client = factory.Client();

        var match = await MatchAsync(factory);
        var file = await LastFileAsync(factory);

        Assert.AreEqual(HttpStatusCode.NotFound, (await client.GetAsync($"/matches/{match.Id + 1000}")).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await client.GetAsync($"/matches/{match.Id + 1000}/statistics/{file.Id}")).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await client.GetAsync($"/matches/{match.Id}/statistics/{file.Id + 1000}")).StatusCode);
    }

    [TestMethod]
    public async Task TheAddressesOfTheOldSiteRedirectForGood()
    {
        using var factory = await FactoryWithMatchAsync();
        using var client = factory.Client();

        var match = await MatchAsync(factory);
        var file = await LastFileAsync(factory);
        var start = StatisticsRowKey.From(match.LastFileDate);
        var end = StatisticsRowKey.From(match.FirstFileDate);

        var list = await client.GetAsync("/last-matches");
        var details = await client.GetAsync($"/last-matches/details/{start}/{end}");
        var statistics = await client.GetAsync($"/last-matches/details/{start}/{end}/statistics/{start}");
        var unknown = await client.GetAsync($"/last-matches/details/{end}1/{end}1");
        var invalid = await client.GetAsync("/last-matches/details/abc/def");
        var unknownFile = await client.GetAsync($"/last-matches/details/{start}/{end}/statistics/{end}1");

        Assert.AreEqual(HttpStatusCode.MovedPermanently, list.StatusCode);
        Assert.AreEqual("/matches", list.Headers.Location!.OriginalString);
        Assert.AreEqual(HttpStatusCode.MovedPermanently, details.StatusCode);
        Assert.AreEqual($"/matches/{match.Id}", details.Headers.Location!.OriginalString);
        Assert.AreEqual(HttpStatusCode.MovedPermanently, statistics.StatusCode);
        Assert.AreEqual($"/matches/{match.Id}/statistics/{file.Id}", statistics.Headers.Location!.OriginalString);
        Assert.AreEqual(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, invalid.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, unknownFile.StatusCode);
    }

    [TestMethod]
    public async Task AnAdministratorFixesTheScoreOfATeamAndTheMatchAndTheRankingFollow()
    {
        using var factory = await FactoryWithMatchAsync();
        using var administrator = factory.Client(HubWebFactory.Administrator);

        var before = await MatchAsync(factory);
        var file = await LastFileAsync(factory);
        var rankingBefore = await factory.RunAsync(services => services.GetRequiredService<IRankingService>().SeasonAsync(before.SeasonId));
        var winnerBefore = rankingBefore.First(player => player.Wins == 1).CommunityId;
        var token = await TokenAsync(administrator, before.Id);

        var response = await PutScoreAsync(administrator, file.Id, 'B', 900, token);
        var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        var after = await MatchAsync(factory);
        var fileAfter = await LastFileAsync(factory);
        var rankingAfter = await factory.RunAsync(services => services.GetRequiredService<IRankingService>().SeasonAsync(after.SeasonId));
        var page = await administrator.GetStringAsync($"/matches/{after.Id}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(before.Id, after.Id);
        Assert.AreEqual(after.Id, result.GetProperty("matchId").GetInt32());
        Assert.AreEqual(file.Id, result.GetProperty("statisticId").GetInt32());
        Assert.AreEqual(600, result.GetProperty("teamAScore").GetInt32());
        Assert.AreEqual(900, result.GetProperty("teamBScore").GetInt32());
        Assert.AreEqual(600, after.TeamAScore);
        Assert.AreEqual(900, after.TeamBScore);
        Assert.AreEqual(600, fileAfter.TeamAScore);
        Assert.AreEqual(900, fileAfter.TeamBScore);
        Assert.AreEqual(1, rankingAfter.First(player => player.CommunityId == winnerBefore).Loss);
        Assert.AreEqual(0, rankingAfter.First(player => player.CommunityId == winnerBefore).Wins);
        Assert.Contains("Team B won by 300 points", page);
        Assert.Contains("&quot;data&quot;:[200,900]", page);
    }

    [TestMethod]
    public async Task OnlyTheGivenTeamChangesWhenAScoreIsFixed()
    {
        using var factory = await FactoryWithMatchAsync();
        using var administrator = factory.Client(HubWebFactory.Administrator);

        var match = await MatchAsync(factory);
        var first = (await FilesAsync(factory))[0];
        var token = await TokenAsync(administrator, match.Id);

        var response = await PutScoreAsync(administrator, first.Id, 'A', 350, token);
        var files = await FilesAsync(factory);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(350, files[0].TeamAScore);
        Assert.AreEqual(200, files[0].TeamBScore);
        Assert.AreEqual(600, files[1].TeamAScore);
        Assert.AreEqual(500, files[1].TeamBScore);
    }

    [TestMethod]
    public async Task NobodyButAnAdministratorFixesAScore()
    {
        using var factory = await FactoryWithMatchAsync();
        using var visitor = factory.Client();
        using var player = factory.Client(HubWebFactory.Player);
        using var administrator = factory.Client(HubWebFactory.Administrator);

        var match = await MatchAsync(factory);
        var file = await LastFileAsync(factory);
        var token = await TokenAsync(administrator, match.Id);

        var anonymous = await PutScoreAsync(visitor, file.Id, 'B', 900, token);
        var forbidden = await PutScoreAsync(player, file.Id, 'B', 900, token);
        var withoutToken = await PutScoreAsync(administrator, file.Id, 'B', 900, null);
        var after = await LastFileAsync(factory);

        Assert.AreEqual(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, forbidden.StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, withoutToken.StatusCode);
        Assert.AreEqual(500, after.TeamBScore);
        Assert.AreEqual(match.Id, (await MatchAsync(factory)).Id);
    }

    [TestMethod]
    [DataRow('A', -1)]
    [DataRow('B', 10001)]
    [DataRow('C', 100)]
    public async Task AnImpossibleScoreOrTeamIsRefused(char team, int score)
    {
        using var factory = await FactoryWithMatchAsync();
        using var administrator = factory.Client(HubWebFactory.Administrator);

        var match = await MatchAsync(factory);
        var file = await LastFileAsync(factory);
        var token = await TokenAsync(administrator, match.Id);

        var response = await PutScoreAsync(administrator, file.Id, team, score, token);
        var after = await LastFileAsync(factory);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.AreEqual(600, after.TeamAScore);
        Assert.AreEqual(500, after.TeamBScore);
    }

    [TestMethod]
    public async Task FixingTheScoreOfAFileThatDoesNotExistIsNotFound()
    {
        using var factory = await FactoryWithMatchAsync();
        using var administrator = factory.Client(HubWebFactory.Administrator);

        var match = await MatchAsync(factory);
        var file = await LastFileAsync(factory);
        var token = await TokenAsync(administrator, match.Id);

        var response = await PutScoreAsync(administrator, file.Id + 1000, 'A', 100, token);

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task TheCampaignsApiListsEveryCampaignWithItsMaps()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client();

        var campaigns = JsonDocument.Parse(await client.GetStringAsync("/api/campaigns")).RootElement;
        var darkCarnival = campaigns.EnumerateArray().Single(campaign => campaign.GetProperty("name").GetString() == "Dark Carnival");

        Assert.IsGreaterThan(10, campaigns.GetArrayLength());
        Assert.AreEqual("c2m1_highway", darkCarnival.GetProperty("maps")[0].GetString());
        Assert.AreEqual(5, darkCarnival.GetProperty("maps").GetArrayLength());
    }

    private static async Task<HubWebFactory> FactoryWithMatchAsync(string serverName = "Test Server")
    {
        var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync(serverName);
        using var server = factory.ServerClient(credentials);

        var now = RecentNight();

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", now, 1, 300, 200));
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", now.AddMinutes(15), 2, 600, 500));

        return factory;
    }

    private static Task<Match> MatchAsync(HubWebFactory factory)
    {
        return factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Matches.AsNoTracking().SingleAsync());
    }

    private static Task<List<StatisticsFile>> FilesAsync(HubWebFactory factory)
    {
        return factory.RunAsync(services => services.GetRequiredService<HubDbContext>().StatisticsFiles.AsNoTracking().OrderBy(file => file.FileDate).ToListAsync());
    }

    private static async Task<StatisticsFile> LastFileAsync(HubWebFactory factory)
    {
        var files = await FilesAsync(factory);

        return files[^1];
    }

    private static async Task<string> TokenAsync(HttpClient administrator, int matchId)
    {
        var body = await administrator.GetStringAsync($"/matches/{matchId}");

        return TokenPattern().Match(body).Groups[1].Value;
    }

    private static Task<HttpResponseMessage> PutScoreAsync(HttpClient client, int fileId, char team, int score, string? token)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/statistics/{fileId}/update-score")
        {
            Content = JsonContent.Create(new { team = team.ToString(), score })
        };

        if (token != null)
            request.Headers.Add(TokenHeader, token);

        return client.SendAsync(request);
    }

    private static DateTime RecentNight()
    {
        var now = DateTime.UtcNow;

        return new DateTime(now.Year, now.Month, Math.Min(now.Day, 20), 12, 0, 0);
    }

    [GeneratedRegex("\"RequestVerificationToken\": \"([^\"]+)\"")]
    private static partial Regex TokenPattern();
}
