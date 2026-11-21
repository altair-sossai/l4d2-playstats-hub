using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Support.Skins;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public partial class PlayerPagesTests
{
    private const long Stranger = 76561198000000999;
    private const long Smurf = 76561198000000998;
    private const string AntiforgeryHeader = "RequestVerificationToken";
    private const string HomeAddress = "200.100.50.1";

    [TestMethod]
    public async Task ThePlayersListShowsTheChosenStatisticAndTheAveragePerGame()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await StartAsync(factory);

        var byExperience = await client.GetStringAsync("/players?orderBy=Experience");
        var byWins = await client.GetStringAsync("/players?orderBy=Wins&asc=True");
        var byGames = await client.GetStringAsync("/players?orderBy=Games");

        Assert.AreEqual(8, PlayerRow().Count(byExperience));
        Assert.Contains("Statistic: <b>Experience</b>", byExperience);
        Assert.Contains("Per game", byExperience);
        Assert.Contains("/players?orderBy=Experience&amp;asc=True", byExperience);
        Assert.Contains($"/player/{StatisticsBuilder.PlayerA1}\"", byExperience);
        Assert.Contains($"/avatar/{StatisticsBuilder.PlayerA1}", byExperience);
        Assert.Contains("Statistic: <b>Wins</b>", byWins);
        Assert.Contains("/players?orderBy=Wins&amp;asc=False", byWins);
        Assert.Contains("&#x2191;", byWins);
        Assert.Contains("&#x2193;", byExperience);
        Assert.DoesNotContain("Per game", byGames);
        Assert.Contains("css/bootstrap.min.css", byExperience);
    }

    [TestMethod]
    public async Task ThePlayersListSortsBothWays()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await StartAsync(factory);

        var descending = PlayerLink().Matches(await client.GetStringAsync("/players?orderBy=Wins&asc=False")).Select(match => match.Groups[1].Value).Distinct().ToList();
        var ascending = PlayerLink().Matches(await client.GetStringAsync("/players?orderBy=Wins&asc=True")).Select(match => match.Groups[1].Value).Distinct().ToList();

        var ranking = await factory.RunAsync(services => services.GetRequiredService<IRankingService>().CurrentAsync());
        var expected = ranking.OrderByDescending(player => player.Wins).ThenByDescending(player => player.CommunityId).Select(player => player.CommunityId.ToString()).ToList();

        CollectionAssert.AreEqual(expected, descending);
        CollectionAssert.AreEqual(Enumerable.Reverse(expected).ToList(), ascending);
    }

    [TestMethod]
    public async Task ThePlayerPageHasEveryBlockOfTheOldPage()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await StartAsync(factory);

        var response = await client.GetAsync($"/player/{StatisticsBuilder.PlayerA1}");
        var html = await response.Content.ReadAsStringAsync();

        string[] blocks =
        [
            "Player details", "id=\"player-tabs\"", "href=\"#general\"", "href=\"#campaigns\"", "href=\"#relations\"",
            "General statistics", "Ranking:", "Steam ID:", "Profile URL:", "Previous names:", "Patent:", "XP:", "progress-bar",
            "Games:", "Wins:", "Rage quit:", "Win rate:", "Loss:", "Punishment:", "Longest win streak:", "Longest loss streak:", "Match history:", "streak-tile",
            "Statistics as survivor", "SI MVP awards:", "CI MVP awards:", "FF LVP awards:", "SI killed:", "Commons killed:", "Damage to SI:", "Deaths:",
            "Times incapacitated:", "SI damage taken:", "Damage to Tank:", "Rock hits taken:", "FF dealt (non-incap.):", "Total skeets:", "Melee skeets:", "Total levels:",
            "Statistics as infected", "SI damage (excl. Tank):", "Damage as Tank (upright):", "Spit damage (upright):", "Total pounce damage:",
            "Last matches", "class=\"app-chart\"", "Scoreboard", "Points earned per map", "Dark Carnival", "campaign-card", "campaign-bar",
            "Player relationships", "relation-card-item", "Playing together", "Playing against", "relation-help-", "Compare...", "players-dropdown-menu",
            "https://steamcommunity.com/profiles/" + StatisticsBuilder.PlayerA1, "copyTextToClipboard", "data-lity"
        ];

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        foreach (var block in blocks)
            Assert.Contains(block, html, block);

        Assert.DoesNotContain("Remove comparison", html);
        Assert.Contains("css/bootstrap.min.css", html);
        Assert.Contains("<meta property=\"og:type\" content=\"profile\">", html);
    }

    [TestMethod]
    public async Task TheMatchCardsOfThePlayerPageKeepThePeriodOnThePlayerLinks()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await StartAsync(factory);

        var opponent = StatisticsBuilder.Sample().Survivors('B')[0];
        var matchId = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Matches.Select(match => match.Id).SingleAsync());

        var allTime = await client.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}?period=all-time");
        var current = await client.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}");
        var matches = await client.GetStringAsync("/matches");

        Assert.Contains($"href=\"/player/{opponent}?period=all-time\"", allTime);
        Assert.Contains($"href=\"/matches/{matchId}\"", allTime);
        Assert.Contains($"href=\"/player/{opponent}\"", current);
        Assert.Contains($"href=\"/player/{opponent}\"", matches);
        Assert.DoesNotContain($"href=\"/player/{opponent}?", matches);
    }

    [TestMethod]
    public async Task TwoPlayersCanBeCompared()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await StartAsync(factory);

        var opponent = StatisticsBuilder.Sample().Survivors('B')[0];
        var teammate = StatisticsBuilder.Sample().Survivors('A').First(id => id != StatisticsBuilder.PlayerA1);
        var against = await client.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}/{opponent}");
        var together = await client.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}/{teammate}");

        Assert.Contains("Remove comparison", against);
        Assert.Contains("class=\"second\"", against);
        Assert.Contains($"/avatar/{opponent}", against);
        Assert.Contains("mobile-row-label\">Playing against:", against);
        Assert.DoesNotContain("mobile-row-label\">Playing together:", against);
        Assert.Contains("mobile-row-label\">Playing together:", together);
        Assert.Contains("panel panel-warning", against);
        Assert.Contains($"/player/{StatisticsBuilder.PlayerA1}\"", against);
        Assert.Contains(" vs ", TitleOf(against));
        Assert.Contains("content=\"noindex, follow\"", against);
        Assert.Contains("content=\"index, follow\"", await client.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}"));
    }

    [TestMethod]
    public async Task TheRobotsFileKeepsCrawlersOffTheComparisonsAndTheSettingLinks()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var robots = await client.GetStringAsync("/robots.txt");
        var home = await client.GetStringAsync("/");

        Assert.Contains("Disallow: /player/*/*", robots);
        Assert.Contains("Disallow: /theme/", robots);
        Assert.Contains("Disallow: /language", robots);
        Assert.Contains("rel=\"nofollow\"", home);
    }

    [TestMethod]
    public async Task APlayerOutsideTheRankingGetsTheNotRankedPage()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await StartAsync(factory);

        var response = await client.GetAsync($"/player/{Stranger}");
        var html = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Player not ranked", html);
        Assert.Contains("Available information", html);
        Assert.Contains("Community ID:", html);
        Assert.Contains("Steam ID:", html);
        Assert.Contains("STEAM_0:1:19867635", html);
        Assert.Contains("STEAM_1:1:19867635", html);
        Assert.Contains("[U:1:39735271]", html);
        Assert.Contains($"href=\"https://steamcommunity.com/profiles/{Stranger}\" class=\"btn btn-primary btn-block player-steam-profile\"", html);
        Assert.Contains("Open Steam profile", html);
        Assert.Contains("Return to ranking page", html);
        Assert.Contains("noindex", html);
        Assert.DoesNotContain("Related accounts", html);
        Assert.DoesNotContain("No statistics yet", html);
    }

    [TestMethod]
    public async Task APlayerWithoutStatisticsShowsTheBestKnownName()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await StartAsync(factory);
        await ConnectAsync(factory, Stranger, "StrangerOnTheServer");

        var html = await client.GetStringAsync($"/player/{Stranger}");

        Assert.Contains("StrangerOnTheServer - Player details", TitleOf(html));
        Assert.Contains("<td data-label=\"Name:\">StrangerOnTheServer</td>", html);
        Assert.Contains("Open Steam profile", html);
    }

    [TestMethod]
    public async Task AnInvalidPlayerAddressIsNotFound()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await StartAsync(factory);

        foreach (var page in new[] { "/player/null", "/player/undefined", "/player/abc", "/player/0", "/player/-1", "/player/5", "/player/76561197960265728", "/player/99999999999999999999", $"/player/{StatisticsBuilder.PlayerA1}/null", $"/player/{StatisticsBuilder.PlayerA1}/0", $"/player/{StatisticsBuilder.PlayerA1}/5" })
        {
            var response = await client.GetAsync(page);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, page);
            Assert.DoesNotContain("Available information", await response.Content.ReadAsStringAsync(), page);
        }
    }

    [TestMethod]
    public async Task AccountsOnTheSameAddressAreListedWhileTheLiveServerIsOn()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await StartAsync(factory);
        await ConnectAsync(factory, StatisticsBuilder.PlayerA1, "Main");
        await ConnectAsync(factory, Smurf, "Smurf account");

        var ranked = await client.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}");
        var unranked = await client.GetStringAsync($"/player/{Smurf}");

        await factory.SaveAsync(new ModuleSettings { LiveServer = false });

        var withoutLiveServer = await client.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}");

        Assert.Contains("Related accounts", ranked);
        Assert.Contains("Smurf account", ranked);
        Assert.Contains($"/player/{Smurf}\"", ranked);
        Assert.Contains("Last login", ranked);
        Assert.Contains("Related accounts", unranked);
        Assert.Contains("Smurf account", unranked);
        Assert.Contains("Connections:", unranked);
        Assert.DoesNotContain("Related accounts", withoutLiveServer);
    }

    [TestMethod]
    public async Task EveryoneSeesTheRelatedAccountsByDefault()
    {
        using var factory = new HubWebFactory();

        await StartAsync(factory);
        await ConnectAsync(factory, StatisticsBuilder.PlayerA1, "Main");
        await ConnectAsync(factory, Smurf, "Smurf account");

        foreach (var user in new long?[] { null, HubWebFactory.Player, HubWebFactory.Administrator })
        {
            using var client = factory.Client(user);

            Assert.Contains("Smurf account", await client.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}"), user?.ToString());
            Assert.Contains("Related accounts", await client.GetStringAsync($"/player/{Smurf}"), user?.ToString());
        }
    }

    [TestMethod]
    public async Task OnlyAdministratorsSeeTheRelatedAccountsWhenTheSettingSaysSo()
    {
        using var factory = new HubWebFactory();

        await StartAsync(factory);
        await ConnectAsync(factory, StatisticsBuilder.PlayerA1, "Main");
        await ConnectAsync(factory, Smurf, "Smurf account");
        await factory.SaveAsync(new PrivacySettings { RelatedAccounts = RelatedAccountsVisibility.Administrators });

        using var visitor = factory.Client();
        using var player = factory.Client(HubWebFactory.Player);
        using var administrator = factory.Client(HubWebFactory.Administrator);

        foreach (var client in new[] { visitor, player })
        {
            var ranked = await client.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}");
            var unranked = await client.GetStringAsync($"/player/{Smurf}");

            Assert.DoesNotContain("Related accounts", ranked);
            Assert.DoesNotContain("Smurf account", ranked);
            Assert.DoesNotContain("Related accounts", unranked);
            Assert.Contains("Player not ranked", unranked);
            Assert.Contains("Connections:", unranked);
        }

        Assert.Contains("Smurf account", await administrator.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}"));
        Assert.Contains("Related accounts", await administrator.GetStringAsync($"/player/{Smurf}"));
    }

    [TestMethod]
    public async Task APastSeasonStaysBrowsable()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        var now = RecentNight();
        var before = now.AddMonths(-7);

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c5m1_waterfront", before, 1, 300, 200));
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c5m2_park", before.AddMinutes(15), 2, 600, 500));
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", now, 1, 300, 200));
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", now.AddMinutes(15), 2, 600, 500));

        var past = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Matches.Where(match => match.Campaign == "The Parish").Select(match => match.SeasonId).SingleAsync());

        var current = await client.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}");
        var old = await client.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}?season={past}");
        var list = await client.GetStringAsync($"/players?season={past}");
        var unknown = await client.GetAsync($"/player/{StatisticsBuilder.PlayerA1}?season=987654");
        var unknownList = await client.GetAsync("/players?season=987654");

        Assert.Contains("Displayed period", current);
        Assert.Contains("Current season", current);
        Assert.Contains("Dark Carnival", current);
        Assert.DoesNotContain("The Parish", current);
        Assert.Contains(before.Year == now.Year ? $"/player/{StatisticsBuilder.PlayerA1}?season={past}" : $"/player/{StatisticsBuilder.PlayerA1}?year={before.Year}", current);
        Assert.Contains($"/player/{StatisticsBuilder.PlayerA1}?year={now.Year}", current);
        Assert.Contains("The Parish", old);
        Assert.DoesNotContain("Dark Carnival", old);
        Assert.Contains($"?season={past}\"", old);
        Assert.Contains($"/player/{StatisticsBuilder.PlayerA1}?season={past}", list);
        Assert.Contains($"/players?orderBy=Wins&amp;asc=False&amp;season={past}", list);
        Assert.AreEqual(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, unknownList.StatusCode);
    }

    [TestMethod]
    public async Task TheSkinsTabExistsOnlyWhileTheSkinShopIsOn()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await StartAsync(factory);

        var without = await client.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}");

        await factory.StartSkinShopAsync();

        var stars = await factory.RunAsync(StarsOracle.CountAsync);
        var player = stars.OrderByDescending(pair => pair.Value).ThenBy(pair => pair.Key).First().Key;

        await factory.ShopAsync(shop => shop.GiftAsync(player, SkinSamples.GoldMagnum, HubWebFactory.Administrator, "Admin"));

        var with = await client.GetStringAsync($"/player/{player}");

        Assert.DoesNotContain("href=\"#skins\"", without);
        Assert.DoesNotContain("id=\"skins\"", without);
        Assert.Contains("href=\"#skins\"", with);
        Assert.Contains("id=\"skins\"", with);
        Assert.Contains("class=\"player-skin ", with);
        Assert.Contains($"/skins#skin-{SkinSamples.GoldMagnum}", with);
        Assert.Contains("player-stars", with);
        Assert.Contains("See the skin shop", with);
    }

    [TestMethod]
    public async Task OnlyAnAdministratorSeesThePunishmentControl()
    {
        using var factory = new HubWebFactory();
        using var anonymous = factory.Client();
        using var player = factory.Client(HubWebFactory.Player);
        using var administrator = factory.Client(HubWebFactory.Administrator);

        await StartAsync(factory);

        var page = $"/player/{StatisticsBuilder.PlayerA1}";

        Assert.DoesNotContain("onclick=\"editPunishment(", await anonymous.GetStringAsync(page));
        Assert.DoesNotContain("onclick=\"editPunishment(", await player.GetStringAsync(page));
        Assert.Contains($"onclick=\"editPunishment('{StatisticsBuilder.PlayerA1}', '150')\"", await administrator.GetStringAsync(page));
    }

    [TestMethod]
    public async Task AnAdministratorPunishesAndForgivesAPlayer()
    {
        using var factory = new HubWebFactory();
        using var administrator = factory.Client(HubWebFactory.Administrator);

        await StartAsync(factory);

        var page = $"/player/{StatisticsBuilder.PlayerA1}";
        var token = Token().Match(await administrator.GetStringAsync(page)).Groups[1].Value;
        var before = await ExperienceAsync(factory);

        var punished = await SendAsync(administrator, HttpMethod.Post, "/api/punishments", token, new { communityId = StatisticsBuilder.PlayerA1.ToString(), lostExperiencePoints = 30 });
        var again = await SendAsync(administrator, HttpMethod.Post, "/api/punishments", token, new { communityId = StatisticsBuilder.PlayerA1.ToString(), lostExperiencePoints = 20 });
        var result = await again.Content.ReadFromJsonAsync<JsonElement>();
        var after = await ExperienceAsync(factory);
        var html = await administrator.GetStringAsync(page);

        var forgiven = await SendAsync(administrator, HttpMethod.Delete, $"/api/punishments/{StatisticsBuilder.PlayerA1}", token, null);
        var restored = await ExperienceAsync(factory);

        Assert.AreEqual(HttpStatusCode.OK, punished.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, again.StatusCode);
        Assert.AreEqual(50, result.GetProperty("lostExperiencePoints").GetInt32());
        Assert.AreEqual(StatisticsBuilder.PlayerA1.ToString(), result.GetProperty("communityId").GetString());
        Assert.AreEqual(Math.Max(0, before - 50), after);
        Assert.Contains("-50 XP", html);
        Assert.AreEqual(HttpStatusCode.NoContent, forgiven.StatusCode);
        Assert.AreEqual(before, restored);
    }

    [TestMethod]
    public async Task ThePunishmentApiRefusesEveryoneElse()
    {
        using var factory = new HubWebFactory();
        using var anonymous = factory.Client();
        using var player = factory.Client(HubWebFactory.Player);
        using var administrator = factory.Client(HubWebFactory.Administrator);

        await StartAsync(factory);

        var token = Token().Match(await administrator.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}")).Groups[1].Value;
        var command = new { communityId = StatisticsBuilder.PlayerA1.ToString(), lostExperiencePoints = 30 };

        var fromAnonymous = await SendAsync(anonymous, HttpMethod.Post, "/api/punishments", token, command);
        var fromPlayer = await SendAsync(player, HttpMethod.Post, "/api/punishments", token, command);
        var deleteFromPlayer = await SendAsync(player, HttpMethod.Delete, $"/api/punishments/{StatisticsBuilder.PlayerA1}", token, null);
        var withoutToken = await SendAsync(administrator, HttpMethod.Post, "/api/punishments", null, command);
        var invalidPlayer = await SendAsync(administrator, HttpMethod.Post, "/api/punishments", token, new { communityId = "abc", lostExperiencePoints = 30 });
        var noPoints = await SendAsync(administrator, HttpMethod.Post, "/api/punishments", token, new { communityId = StatisticsBuilder.PlayerA1.ToString(), lostExperiencePoints = 0 });
        var unknownSeason = await SendAsync(administrator, HttpMethod.Post, "/api/punishments", token, new { communityId = StatisticsBuilder.PlayerA1.ToString(), lostExperiencePoints = 30, seasonId = 987654 });

        var punishments = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Punishments.CountAsync());

        Assert.AreEqual(HttpStatusCode.Unauthorized, fromAnonymous.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, fromPlayer.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, deleteFromPlayer.StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, withoutToken.StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, invalidPlayer.StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, noPoints.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, unknownSeason.StatusCode);
        Assert.AreEqual(0, punishments);
    }

    [TestMethod]
    public async Task WithoutTheRankingThePlayerPageShowsOnlyTheBasicPage()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await StartAsync(factory);
        await factory.SaveAsync(new ModuleSettings { Ranking = false });

        Assert.AreEqual(HttpStatusCode.NotFound, (await client.GetAsync("/players")).StatusCode);

        foreach (var page in new[] { $"/player/{StatisticsBuilder.PlayerA1}", $"/player/{StatisticsBuilder.PlayerA1}/{Stranger}", $"/player/{Stranger}" })
        {
            var response = await client.GetAsync(page);
            var html = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, page);
            Assert.Contains("No statistics yet", html, page);
            Assert.Contains("Open Steam profile", html, page);
            Assert.Contains("https://steamcommunity.com/profiles/", html, page);
            Assert.Contains("Back to the home page", html, page);
            Assert.DoesNotContain("Player not ranked", html, page);
            Assert.DoesNotContain("Return to ranking page", html, page);
            Assert.DoesNotContain("General statistics", html, page);
        }

        Assert.AreEqual(HttpStatusCode.NotFound, (await client.GetAsync("/player/null")).StatusCode);
    }

    private static async Task StartAsync(HubWebFactory factory)
    {
        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        var now = RecentNight();

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", now, 1, 300, 200));
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", now.AddMinutes(15), 2, 600, 500));
    }

    private static Task ConnectAsync(HubWebFactory factory, long communityId, string name)
    {
        var command = new PlayerConnectionCommand { CommunityId = communityId.ToString(), IpAddress = HomeAddress, Name = name };

        return factory.RunAsync(services => services.GetRequiredService<IPlayerConnectionService>().RegisterAsync(command));
    }

    private static async Task<decimal> ExperienceAsync(HubWebFactory factory)
    {
        var ranking = await factory.RunAsync(services => services.GetRequiredService<IRankingService>().CurrentAsync());

        return ranking.Single(player => player.CommunityId == StatisticsBuilder.PlayerA1).Experience;
    }

    private static Task<HttpResponseMessage> SendAsync(HttpClient client, HttpMethod method, string url, string? token, object? body)
    {
        var request = new HttpRequestMessage(method, url);

        if (token != null)
            request.Headers.Add(AntiforgeryHeader, token);

        if (body != null)
            request.Content = JsonContent.Create(body);

        return client.SendAsync(request);
    }

    private static string TitleOf(string html)
    {
        return Title().Match(html).Groups[1].Value;
    }

    private static DateTime RecentNight()
    {
        var now = DateTime.UtcNow;

        return new DateTime(now.Year, now.Month, Math.Min(now.Day, 20), 12, 0, 0);
    }

    [GeneratedRegex("<tr class=\"hover")]
    private static partial Regex PlayerRow();

    [GeneratedRegex("<a href=\"/player/(\\d+)\">")]
    private static partial Regex PlayerLink();

    [GeneratedRegex("\"RequestVerificationToken\": \"([^\"]+)\"")]
    private static partial Regex Token();

    [GeneratedRegex("<title>([^<]*)</title>")]
    private static partial Regex Title();
}
