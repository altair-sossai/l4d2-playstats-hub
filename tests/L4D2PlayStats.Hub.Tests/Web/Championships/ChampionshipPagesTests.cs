using System.Net;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Web.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web.Championships;

[TestClass]
public partial class ChampionshipPagesTests
{
    private const long FirstPlayer = 76561198100000000;
    private const long Stranger = 76561198199999999;

    [TestMethod]
    public async Task TheModuleIsOffByDefaultAndItsPagesAnswerNotFound()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var admin = factory.Client(HubWebFactory.Administrator);

        Assert.AreEqual(HttpStatusCode.NotFound, (await admin.GetAsync("/championships")).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await admin.GetAsync("/admin/championships")).StatusCode);
        Assert.DoesNotContain("href=\"/championships\"", await admin.GetStringAsync("/"));

        await factory.SaveAsync(new ModuleSettings { Ranking = true, LiveServer = true, Championships = true });

        var home = await admin.GetStringAsync("/");

        Assert.Contains("href=\"/championships\"", home);
        Assert.AreEqual(HttpStatusCode.OK, (await admin.GetAsync("/championships")).StatusCode);
        Assert.Contains("<a href=\"/admin/championships\"", await admin.GetStringAsync("/admin"));
    }

    [TestMethod]
    public async Task AChampionshipStaysHiddenWhileItIsADraft()
    {
        using var factory = await FactoryAsync();
        using var admin = factory.Client(HubWebFactory.Administrator);
        using var visitor = factory.Client();

        var created = await HubWebFactory.PostFormAsync(admin, "/admin/championships", "/admin/championships", new Dictionary<string, string> { ["name"] = "Copa Secreta", ["template"] = "Knockout" });

        Assert.AreEqual(HttpStatusCode.Redirect, created.StatusCode);
        Assert.MatchesRegex(@"^/admin/championships/\d+$", created.Headers.Location!.OriginalString);

        Assert.AreEqual(HttpStatusCode.NotFound, (await visitor.GetAsync("/championships/copa-secreta")).StatusCode);
        Assert.DoesNotContain("Copa Secreta", await visitor.GetStringAsync("/championships"));

        var preview = await admin.GetStringAsync("/championships/copa-secreta");

        Assert.Contains("Only administrators can see it.", preview);

        var location = created.Headers.Location.OriginalString;
        var status = await HubWebFactory.PostFormAsync(admin, location, location, new Dictionary<string, string> { ["Name"] = "Copa Secreta", ["Slug"] = "copa-secreta", ["PlayersPerGame"] = "4", ["RosterSize"] = "4", ["DraftMethod"] = "Manual", ["status"] = "Participants" });

        Assert.AreEqual(HttpStatusCode.Redirect, status.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, (await visitor.GetAsync("/championships/copa-secreta")).StatusCode);
        Assert.Contains("Copa Secreta", await visitor.GetStringAsync("/championships"));
    }

    [TestMethod]
    public async Task AChampionshipIsRunFromTheAdministrationAndShownOnEveryPublicPage()
    {
        using var factory = await FactoryAsync();
        using var admin = factory.Client(HubWebFactory.Administrator);
        using var visitor = factory.Client();

        var created = await HubWebFactory.PostFormAsync(admin, "/admin/championships", "/admin/championships", new Dictionary<string, string> { ["name"] = "Liga Teste", ["template"] = "RoundRobinAndKnockout" });
        var url = created.Headers.Location!.OriginalString;
        var id = int.Parse(url.Split('/').Last());

        await HubWebFactory.PostFormAsync(admin, url, url, new Dictionary<string, string>
        {
            ["Name"] = "Liga Teste", ["Slug"] = "liga-teste", ["PlayersPerGame"] = "2", ["RosterSize"] = "2", ["DraftMethod"] = "Manual", ["Substitutes"] = "false",
            ["startsOn"] = "2026-12-01", ["endsOn"] = "2026-12-20", ["StreamUrl"] = "https://www.twitch.tv/example"
        });

        var rulesSaved = await HubWebFactory.PostFormAsync(admin, $"{url}?tab=rules", $"{url}/rules", new Dictionary<string, string> { ["rules"] = "# Format\n- Every team plays **every** other team", ["rulesPortuguese"] = "# Formato" });

        Assert.AreEqual($"{url}?tab=rules", rulesSaved.Headers.Location!.OriginalString);

        var players = string.Join('\n', Enumerable.Range(0, 8).Select(index => (FirstPlayer + index).ToString()));
        var added = await HubWebFactory.PostFormAsync(admin, url, $"{url}/participants", new Dictionary<string, string> { ["players"] = players });

        Assert.AreEqual(HttpStatusCode.Redirect, added.StatusCode);

        await HubWebFactory.PostFormAsync(admin, url, $"{url}/teams/many", new Dictionary<string, string> { ["count"] = "4" });

        var championship = await LoadAsync(factory, id);
        var teams = championship.Teams.OrderBy(team => team.Seed).ToList();

        Assert.HasCount(8, championship.Participants);
        Assert.HasCount(4, teams);

        for (var index = 0; index < 8; index++)
            await HubWebFactory.PostFormAsync(admin, url, $"{url}/teams/{teams[index / 2].Id}/members", new Dictionary<string, string> { ["player"] = (FirstPlayer + index).ToString() });

        await HubWebFactory.PostFormAsync(admin, url, $"{url}/teams/{teams[0].Id}", new Dictionary<string, string> { ["name"] = "Team A", ["seed"] = "1", ["captain"] = FirstPlayer.ToString(), ["viceCaptain"] = (FirstPlayer + 1).ToString() });

        foreach (var page in new[] { "/admin/championships", "/admin/championships?page=2", url, $"{url}?tab=rules", $"{url}?tab=participants&participantsPage=2", $"{url}?tab=teams", $"{url}?tab=stages", $"{url}?tab=games", $"{url}?tab=history&historyPage=3" })
            Assert.AreEqual(HttpStatusCode.OK, (await admin.GetAsync(page)).StatusCode, page);

        var settingsPage = await admin.GetStringAsync(url);

        Assert.Contains("id=\"RulesPortuguese\"", settingsPage);
        Assert.Contains($"action=\"{url}/rules\"", settingsPage);
        Assert.AreEqual(1, Regex.Matches(settingsPage, "id=\"Rules\"").Count);

        championship = await LoadAsync(factory, id);

        var stages = championship.Stages.OrderBy(stage => stage.Order).ToList();
        var preview = await admin.GetStringAsync($"{url}?tab=stages");

        Assert.Contains("Teams that enter", preview);

        var built = await HubWebFactory.PostFormAsync(admin, url, $"{url}/stages/{stages[0].Id}/generate", []);

        Assert.AreEqual(HttpStatusCode.Redirect, built.StatusCode);

        championship = await LoadAsync(factory, id);

        Assert.AreEqual(ChampionshipStatus.Running, championship.Status);

        var league = championship.Stages.First(stage => stage.Id == stages[0].Id);

        foreach (var series in league.Series)
        {
            var seedA = teams.FindIndex(team => team.Id == series.TeamAId);
            var seedB = teams.FindIndex(team => team.Id == series.TeamBId);
            var response = await HubWebFactory.PostFormAsync(admin, url, $"{url}/series/{series.Id}/games/{series.Games[0].Id}", new Dictionary<string, string>
            {
                ["result"] = "typed",
                ["teamAScore"] = seedA < seedB ? "800" : "200",
                ["teamBScore"] = seedA < seedB ? "200" : "800"
            });

            Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        }

        await HubWebFactory.PostFormAsync(admin, url, $"{url}/stages/{stages[1].Id}/generate", []);

        championship = await LoadAsync(factory, id);

        var playoffs = championship.Stages.First(stage => stage.Id == stages[1].Id);

        Assert.IsTrue(playoffs.Generated);
        Assert.AreEqual(teams[0].Id, playoffs.Series.Single(series => series.Round == 1 && series.Position == 1).TeamAId);

        var slug = "/championships/liga-teste";

        foreach (var page in new[] { slug, $"{slug}/rules", $"{slug}/teams", $"{slug}/standings", $"{slug}/bracket", $"{slug}/games", $"{slug}/statistics", $"{slug}/series/{playoffs.Series[0].Id}" })
        {
            var response = await visitor.GetAsync(page);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, page);
            Assert.Contains("Liga Teste", await response.Content.ReadAsStringAsync(), page);
        }

        var rules = await visitor.GetStringAsync($"{slug}/rules");
        var standings = await visitor.GetStringAsync($"{slug}/standings");
        var overview = await visitor.GetStringAsync(slug);

        Assert.Contains("<h3>Format</h3>", rules);
        Assert.Contains("<strong>every</strong>", rules);
        Assert.Contains("championship-qualified", standings);
        Assert.Contains("https://www.twitch.tv/example", overview);
        Assert.AreEqual(HttpStatusCode.NotFound, (await visitor.GetAsync($"{slug}/unknown")).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await visitor.GetAsync($"{slug}/draft/x")).StatusCode);

        var log = await admin.GetStringAsync($"{url}?tab=history");

        Assert.Contains("Built the stage", log);
    }

    [TestMethod]
    public async Task CaptainsChooseTheCampaignsAndAgreeOnADateFromTheCaptainArea()
    {
        using var factory = await FactoryAsync();

        var (slug, seriesId, games) = await KnockoutAsync(factory);
        var page = $"/championships/{slug}/series/{seriesId}";
        var area = $"/championships/{slug}/captain";
        var series = $"{area}/series/{seriesId}";

        using var captainA = factory.Client(FirstPlayer);
        using var viceB = factory.Client(FirstPlayer + 3);
        using var stranger = factory.Client(Stranger);

        var asCaptain = await captainA.GetStringAsync(area);
        var publicPage = await captainA.GetStringAsync(page);

        Assert.Contains("Captain area: Team A", asCaptain);
        Assert.Contains($"action=\"{series}/games/{games[0]}/pick\"", asCaptain);
        Assert.DoesNotContain($"action=\"{series}/games/{games[1]}/pick\"", asCaptain);
        Assert.Contains("Choose the campaign of game 1 against Team B", asCaptain);
        Assert.Contains("Propose a date to Team B", asCaptain);
        Assert.Contains($"href=\"{area}\"", publicPage);
        Assert.DoesNotContain("/pick\"", publicPage);
        Assert.Contains("nav-captain-badge", publicPage);
        Assert.AreEqual(HttpStatusCode.NotFound, (await stranger.GetAsync(area)).StatusCode);
        Assert.DoesNotContain("nav-captain", await stranger.GetStringAsync(page));
        Assert.DoesNotContain($"href=\"{area}\"", await stranger.GetStringAsync(page));

        var campaignId = Regex.Match(asCaptain, $"id=\"captain-pick-{games[0]}\" name=\"campaignId\">\\s*<option value=\"(\\d+)\"").Groups[1].Value;
        var picked = await HubWebFactory.PostFormAsync(captainA, area, $"{series}/games/{games[0]}/pick", new Dictionary<string, string> { ["campaignId"] = campaignId });

        Assert.AreEqual(HttpStatusCode.Redirect, picked.StatusCode);
        Assert.AreEqual($"{area}?tab=series", picked.Headers.Location!.OriginalString);

        var date = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddT21:00");
        var proposed = await HubWebFactory.PostFormAsync(captainA, area, $"{series}/proposals", new Dictionary<string, string> { ["date"] = date });

        Assert.AreEqual(HttpStatusCode.Redirect, proposed.StatusCode);

        var strangerPost = await HubWebFactory.PostFormAsync(stranger, page, $"{series}/proposals", new Dictionary<string, string> { ["date"] = DateTime.UtcNow.AddDays(4).ToString("yyyy-MM-ddT21:00") });

        Assert.AreEqual(HttpStatusCode.NotFound, strangerPost.StatusCode);

        var championship = await LoadAsync(factory, await IdAsync(factory, slug));
        var loaded = championship.Stages.Single().Series.Single(item => item.Id == seriesId);
        var proposal = loaded.Proposals.Single();

        Assert.AreEqual(int.Parse(campaignId), loaded.Games.Single(game => game.Id == games[0]).CampaignId);

        var asVice = await viceB.GetStringAsync(area);

        Assert.Contains("Answer the date proposed by Team A", asVice);
        Assert.Contains($"action=\"{series}/proposals/{proposal.Id}/answer\"", asVice);

        var answered = await HubWebFactory.PostFormAsync(viceB, area, $"{series}/proposals/{proposal.Id}/answer", new Dictionary<string, string> { ["accept"] = "true" });

        Assert.AreEqual(HttpStatusCode.Redirect, answered.StatusCode);

        var confirmed = await stranger.GetStringAsync(page);

        Assert.Contains("calendar.google.com", confirmed);
        Assert.Contains("Confirmed", await viceB.GetStringAsync(area));
        Assert.DoesNotContain("Propose a date to Team A", await viceB.GetStringAsync(area));

        var calendar = await stranger.GetAsync($"{page}/calendar.ics");

        Assert.AreEqual("text/calendar", calendar.Content.Headers.ContentType!.MediaType);
        Assert.Contains("BEGIN:VEVENT", await calendar.Content.ReadAsStringAsync());

        using var signedOut = factory.Client();

        var refused = await HubWebFactory.PostFormAsync(signedOut, page, $"{series}/proposals", new Dictionary<string, string> { ["date"] = date });

        Assert.AreNotEqual(HttpStatusCode.OK, refused.StatusCode);
        Assert.HasCount(1, (await LoadAsync(factory, await IdAsync(factory, slug))).Stages.Single().Series.Single(item => item.Id == seriesId).Proposals);
    }

    [TestMethod]
    public async Task TheMatchPageShowsTheChampionshipOfItsGame()
    {
        using var factory = await FactoryAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        var night = DateTime.UtcNow.AddDays(-1);
        var first = StatisticsBuilder.Map("c2m1_highway", night, 1, 300, 200);

        await HubWebFactory.UploadAsync(server, first);
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", night.AddMinutes(15), 2, 450, 900));

        var matchId = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Matches.Select(match => match.Id).SingleAsync());
        var (slug, seriesId, games) = await factory.RunAsync(async services =>
        {
            var championships = services.GetRequiredService<IChampionshipService>();
            var teamService = services.GetRequiredService<IChampionshipTeamService>();
            var championship = await championships.CreateAsync("Copa Partida", [new ChampionshipStageTemplate("Final", ChampionshipStageType.Knockout, 1, 0, 1, ChampionshipCampaignRule.Manual, ChampionshipSeeding.Seeds)], HubWebFactory.Administrator);
            var blue = await teamService.CreateTeamAsync(championship.Id, "Blue", null, null);
            var red = await teamService.CreateTeamAsync(championship.Id, "Red", null, null);

            foreach (var player in first.Survivors('A'))
                await teamService.AddMemberAsync(blue.Id, player, null);

            foreach (var player in first.Survivors('B'))
                await teamService.AddMemberAsync(red.Id, player, null);

            return (championship.Slug, 0, new List<int>());
        });

        var id = await IdAsync(factory, slug);
        var stageId = (await LoadAsync(factory, id)).Stages.Single().Id;

        await factory.RunAsync(services => services.GetRequiredService<IChampionshipStageService>().GenerateAsync(stageId, null));

        var game = (await LoadAsync(factory, id)).Stages.Single().Series.Single().Games.Single();

        await factory.RunAsync(services => services.GetRequiredService<IChampionshipSeriesService>().LinkMatchAsync(game.Id, matchId, null));

        using var visitor = factory.Client();

        var details = await visitor.GetStringAsync($"/matches/{matchId}");

        Assert.Contains($"/championships/{slug}/series/{game.SeriesId}", details);
        Assert.Contains("Copa Partida", details);
        Assert.AreEqual(0, seriesId);
        Assert.IsEmpty(games);

        var series = await visitor.GetStringAsync($"/championships/{slug}/series/{game.SeriesId}");

        Assert.Contains($"href=\"/matches/{matchId}\"", series);

        var statistics = await visitor.GetStringAsync($"/championships/{slug}/statistics");

        Assert.Contains("championship-statistics", statistics);
    }

    [TestMethod]
    public void TheRulesAreRenderedAsSafeMarkdown()
    {
        var html = ChampionshipMarkdown.Render("## Format\n- one **two** *three*\n- [site](https://example.com) [bad](javascript:alert(1))\n\n<script>alert(1)</script>\n1. first").ToString()!;

        Assert.Contains("<h4>Format</h4>", html);
        Assert.Contains("<ul><li>one <strong>two</strong> <em>three</em></li>", html);
        Assert.Contains("<a href=\"https://example.com/\" target=\"_blank\" rel=\"noopener nofollow\">site</a>", html);
        Assert.DoesNotContain("javascript:", html);
        Assert.Contains("&lt;script&gt;", html);
        Assert.Contains("<ol><li>first</li></ol>", html);
    }

    [TestMethod]
    public void EveryMessageOfTheChampionshipsIsTranslated()
    {
        var root = Root();
        var texts = new HashSet<string>();
        var core = Path.Combine(root, "src", "L4D2PlayStats.Hub.Core", "Modules", "Championships");
        var web = Path.Combine(root, "src", "L4D2PlayStats.Hub.Web");

        foreach (var file in Directory.EnumerateFiles(core, "*.cs", SearchOption.AllDirectories).Append(Path.Combine(web, "Controllers", "ChampionshipsController.cs")).Append(Path.Combine(web, "Controllers", "ChampionshipCaptainController.cs")).Append(Path.Combine(web, "Services", "ChampionshipImageService.cs")))
        {
            var text = File.ReadAllText(file);

            foreach (Match match in Exception().Matches(text))
                texts.Add(Regex.Unescape(match.Groups[1].Value));

            foreach (Match match in Log().Matches(text))
                texts.Add(Regex.Unescape(match.Groups[1].Value));

            foreach (Match match in Conditional().Matches(text))
            {
                texts.Add(Regex.Unescape(match.Groups[1].Value));
                texts.Add(Regex.Unescape(match.Groups[2].Value));
            }
        }

        foreach (var file in new[] { Path.Combine(web, "Helpers", "ChampionshipTexts.cs"), Path.Combine(web, "Controllers", "Admin", "ChampionshipsAdminController.cs") })
            foreach (Match match in Localized().Matches(File.ReadAllText(file)))
                texts.Add(Regex.Unescape(match.Groups[1].Value));

        foreach (var culture in new[] { "pt-BR", "es-ES" })
        {
            var translations = Directory.EnumerateFiles(Path.Combine(web, "Resources"), $"{culture}*.json")
                .SelectMany(file => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(file))!.Keys)
                .ToHashSet();
            var missing = texts.Where(text => !translations.Contains(text)).Order().ToList();

            Assert.IsEmpty(missing, $"{culture}: {string.Join(" | ", missing)}");
        }

        Assert.IsGreaterThan(80, texts.Count);
    }

    private static async Task<HubWebFactory> FactoryAsync()
    {
        var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { Ranking = true, LiveServer = true, Championships = true });

        return factory;
    }

    private static async Task<(string Slug, int SeriesId, List<int> Games)> KnockoutAsync(HubWebFactory factory)
    {
        return await factory.RunAsync(async services =>
        {
            var championships = services.GetRequiredService<IChampionshipService>();
            var teamService = services.GetRequiredService<IChampionshipTeamService>();
            var stageService = services.GetRequiredService<IChampionshipStageService>();
            var loader = services.GetRequiredService<IChampionshipLoadService>();
            var championship = await championships.CreateAsync("Copa Capitães", [new ChampionshipStageTemplate("Final", ChampionshipStageType.Knockout, 1, 0, 3, ChampionshipCampaignRule.Picks, ChampionshipSeeding.Seeds)], HubWebFactory.Administrator);

            await championships.UpdateAsync(championship.Id, new ChampionshipSettingsForm { Name = "Copa Capitães", PlayersPerGame = 2, RosterSize = 2 }, null);

            var teamA = await teamService.CreateTeamAsync(championship.Id, "Team A", null, null);
            var teamB = await teamService.CreateTeamAsync(championship.Id, "Team B", null, null);

            await teamService.SetLeadersAsync(teamA.Id, FirstPlayer, FirstPlayer + 1, null);
            await teamService.SetLeadersAsync(teamB.Id, FirstPlayer + 2, FirstPlayer + 3, null);

            var stageId = (await loader.LoadAsync(championship.Id, false))!.Stages.Single().Id;

            await stageService.GenerateAsync(stageId, null);

            var series = (await loader.LoadAsync(championship.Id, false))!.Stages.Single().Series.Single();

            return (championship.Slug, series.Id, series.Games.OrderBy(game => game.Number).Select(game => game.Id).ToList());
        });
    }

    private static Task<L4D2PlayStats.Hub.Core.Modules.Championships.Championship> LoadAsync(HubWebFactory factory, int id)
    {
        return factory.RunAsync(async services => (await services.GetRequiredService<IChampionshipLoadService>().LoadAsync(id, false))!);
    }

    private static Task<int> IdAsync(HubWebFactory factory, string slug)
    {
        return factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Championships.Where(item => item.Slug == slug).Select(item => item.Id).SingleAsync());
    }

    private static string Root()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "src", "L4D2PlayStats.Hub.Web")))
            directory = directory.Parent;

        return directory!.FullName;
    }

    [GeneratedRegex("""ChampionshipException\("((?:[^"\\]|\\.)*)"\)""")]
    private static partial Regex Exception();

    [GeneratedRegex(""""log\.Add\([^"]*?"((?:[^"\\]|\\.)*)"""")]
    private static partial Regex Log();

    [GeneratedRegex(""""\? "((?:[^"\\]|\\.)*)" : "((?:[^"\\]|\\.)*)"""")]
    private static partial Regex Conditional();

    [GeneratedRegex("""L\["((?:[^"\\]|\\.)*)"\s*[\],]""")]
    private static partial Regex Localized();
}
