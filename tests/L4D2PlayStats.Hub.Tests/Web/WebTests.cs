using System.Net;
using System.Text.Json;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Administrators.Services;
using L4D2PlayStats.Hub.Core.Modules.Servers.Enums;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class WebTests
{
    private static readonly DateTime Night = new(2026, 11, 14, 21, 0, 0);

    [TestMethod]
    public async Task BeforeSetupEveryPageLeadsToTheWizard()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        var home = await client.GetAsync("/");
        var ping = await client.GetAsync("/api/ping");
        var setup = await client.GetAsync("/setup");

        Assert.AreEqual(HttpStatusCode.Redirect, home.StatusCode);
        Assert.AreEqual("/setup", home.Headers.Location!.OriginalString);
        Assert.AreEqual(HttpStatusCode.OK, ping.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, setup.StatusCode);
    }

    [TestMethod]
    public async Task TheSetupCodeMakesTheFirstAdministrator()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Player);

        var token = await factory.RunAsync(services => services.GetRequiredService<ISetupService>().EnsureTokenAsync());
        var start = await HubWebFactory.PostFormAsync(client, "/setup", "/setup/start", new Dictionary<string, string> { ["token"] = token! });
        var claim = await client.GetAsync("/setup/claim");
        var administrators = await factory.RunAsync(services => services.GetRequiredService<IAdministratorService>().ListAsync());

        Assert.AreEqual(HttpStatusCode.Unauthorized, start.StatusCode);
        Assert.AreEqual(HttpStatusCode.Redirect, claim.StatusCode);
        Assert.HasCount(1, administrators);
        Assert.AreEqual(HubWebFactory.Player, administrators[0].CommunityId);
    }

    [TestMethod]
    public async Task AWrongSetupCodeMakesNobodyAnAdministrator()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Player);

        await factory.RunAsync(services => services.GetRequiredService<ISetupService>().EnsureTokenAsync());

        var start = await HubWebFactory.PostFormAsync(client, "/setup", "/setup/start", new Dictionary<string, string> { ["token"] = "wrong" });
        var body = await start.Content.ReadAsStringAsync();

        await client.GetAsync("/setup/claim");

        var administrators = await factory.RunAsync(services => services.GetRequiredService<IAdministratorService>().ListAsync());

        Assert.AreEqual(HttpStatusCode.OK, start.StatusCode);
        Assert.Contains("alert-danger", body);
        Assert.IsEmpty(administrators);
    }

    [TestMethod]
    public async Task TheSetupCodeStopsWorkingAfterTheFirstAdministrator()
    {
        using var factory = new HubWebFactory();

        var token = await factory.RunAsync(services => services.GetRequiredService<ISetupService>().EnsureTokenAsync());

        await factory.RunAsync(services => services.GetRequiredService<ISetupService>().ClaimAsync(token, HubWebFactory.Administrator, "Admin"));

        var second = await factory.RunAsync(services => services.GetRequiredService<ISetupService>().ClaimAsync(token, HubWebFactory.Player, "Player"));

        Assert.IsFalse(second);
    }

    [TestMethod]
    public async Task AfterSetupTheHomePageShowsTheRanking()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var home = await client.GetAsync("/");

        Assert.AreEqual(HttpStatusCode.OK, home.StatusCode);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("unknown:key")]
    [DataRow("test-server:wrong")]
    public async Task AGameServerWithoutAValidTokenIsRefused(string? token)
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CreateServerAsync();

        if (token != null)
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);

        var heartbeat = await client.PostAsJsonAsync("/api/servers/heartbeat", new { });
        var upload = await client.PostAsJsonAsync("/api/statistics", new { fileName = "x.txt", content = "x" });

        Assert.AreEqual(HttpStatusCode.Unauthorized, heartbeat.StatusCode);
        Assert.AreEqual(HttpStatusCode.Unauthorized, upload.StatusCode);
    }

    [TestMethod]
    public async Task AHeartbeatPutsTheServerOnline()
    {
        using var factory = new HubWebFactory();

        var credentials = await factory.CreateServerAsync();
        using var client = factory.ServerClient(credentials);

        var response = await client.PostAsJsonAsync("/api/servers/heartbeat", new
        {
            hostname = "Test",
            statisticsWriting = true,
            plugins = new Dictionary<string, string> { ["l4d2_playstats_sync"] = "2.0.0" }
        });

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var status = factory.Services.GetRequiredService<IGameServerPresence>().GetStatus(credentials.Server);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("Test Server", body.GetProperty("server").GetString());
        Assert.AreEqual(GameServerConnectionState.Online, status.State);
        Assert.IsTrue(status.Signal!.StatisticsWriting);
        Assert.IsTrue(status.Signal.Plugins.ContainsKey("l4d2_playstats_sync"));
    }

    [TestMethod]
    public async Task AWrongKeyShowsUpInTheServerStatus()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        var credentials = await factory.CreateServerAsync();

        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"{credentials.Server.PublicId}:wrong");
        await client.PostAsJsonAsync("/api/servers/heartbeat", new { });

        var status = factory.Services.GetRequiredService<IGameServerPresence>().GetStatus(credentials.Server);

        Assert.AreEqual(GameServerConnectionState.InvalidKey, status.State);
    }

    [TestMethod]
    public async Task ThePluginUploadsAFileAndIsToldToDeleteIt()
    {
        using var factory = new HubWebFactory();

        var credentials = await factory.CreateServerAsync();
        using var client = factory.ServerClient(credentials);

        var response = await HubWebFactory.UploadAsync(client, StatisticsBuilder.Sample());
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var files = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().StatisticsFiles.CountAsync());

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsTrue(body.GetProperty("mustBeDeleted").GetBoolean());
        Assert.AreEqual(StatisticsBuilder.SampleFileName, body.GetProperty("fileName").GetString());
        Assert.AreEqual(1, files);
    }

    [TestMethod]
    public async Task AFileThatIsNotReadyYetIsRefusedSoThePluginTriesAgain()
    {
        using var factory = new HubWebFactory();

        var credentials = await factory.CreateServerAsync();
        using var client = factory.ServerClient(credentials);

        var now = DateTime.UtcNow;
        var builder = StatisticsBuilder.Map("c2m1_highway", now, 1, 300, 200);
        var content = builder.Content;
        var firstHalf = content[..content.IndexOf("[RoundHalf:1]", StringComparison.Ordinal)];

        var response = await client.PostAsJsonAsync("/api/statistics", new { fileName = builder.FileName, content = firstHalf });

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task UploadedMatchesShowUpInTheRankingApi()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);
        using var client = factory.Client();

        var now = RecentNight();

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", now, 1, 300, 200));
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", now.AddMinutes(15), 2, 600, 500));

        var ranking = await client.GetFromJsonAsync<JsonElement>("/api/ranking");
        var patents = await client.GetFromJsonAsync<JsonElement>("/api/players/patents");
        var first = ranking[0];

        Assert.AreEqual(8, ranking.GetArrayLength());
        Assert.AreEqual(1, first.GetProperty("position").GetInt32());
        Assert.AreEqual(1, first.GetProperty("wins").GetInt32());
        Assert.AreEqual(JsonValueKind.String, first.GetProperty("communityId").ValueKind);
        Assert.AreEqual(8, patents.GetArrayLength());
    }

    [TestMethod]
    public async Task EveryPublicPageOpens()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);
        using var client = factory.Client();

        var now = RecentNight();

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", now, 1, 300, 200));
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", now.AddMinutes(15), 2, 600, 500));

        var match = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Matches.SingleAsync());

        string[] pages =
        [
            "/", "/matches", $"/matches/{match.Id}", "/players", "/patents", "/history", "/history/all-time", $"/history/{now.Year}",
            $"/player/{StatisticsBuilder.PlayerA1}", "/ingame", $"/ingame?steamid={StatisticsBuilder.PlayerA1}",
            $"/ingame/player?steamid={StatisticsBuilder.PlayerA1}", "/ingame/matches"
        ];

        foreach (var page in pages)
        {
            var response = await client.GetAsync(page);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, page);
        }
    }

    [TestMethod]
    public async Task ThePagesForTheGameUseNothingTheOldBrowserLacks()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);
        using var client = factory.Client();

        var now = RecentNight();

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", now, 1, 300, 200));
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", now.AddMinutes(15), 2, 600, 500));

        string[] forbidden = ["<script", "display: flex", "display:flex", "display: grid", "display:grid", "var(--", "calc(", "vh;", "vw;", "<link", ".webp", ".avif"];

        foreach (var page in new[] { "/ingame", $"/ingame/player?steamid={StatisticsBuilder.PlayerA1}", "/ingame/matches" })
        {
            var html = await client.GetStringAsync(page);

            foreach (var text in forbidden)
                Assert.DoesNotContain(text, html, $"{page} contains {text}");
        }
    }

    [TestMethod]
    public async Task TheMatchesForTheGameShowTheDateInTheLanguageAndTimeZoneOfTheSite()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);
        using var client = factory.Client();

        var now = RecentNight();

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", now, 1, 300, 200));
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", now.AddMinutes(15), 2, 600, 500));
        await factory.SaveAsync(new SiteSettings { TimeZone = "America/Sao_Paulo", DefaultCulture = "pt-BR" });

        var last = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Matches.Select(match => match.LastFileDate).SingleAsync());
        var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(last, DateTimeKind.Utc), TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo"));
        var html = await client.GetStringAsync("/ingame/matches");

        Assert.Contains(local.ToString("dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture), html);
    }

    [TestMethod]
    public async Task ThePageForTheGameHighlightsThePlayerWhoOpenedIt()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);
        using var client = factory.Client();

        var now = RecentNight();

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", now, 1, 300, 200));
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", now.AddMinutes(15), 2, 600, 500));

        var anonymous = await client.GetStringAsync("/ingame");
        var mine = await client.GetStringAsync($"/ingame?steamid={StatisticsBuilder.PlayerA1}");

        Assert.DoesNotContain("class=\"me\"", anonymous);
        Assert.Contains("class=\"me\"", mine);
    }

    [TestMethod]
    public async Task AModuleThatIsOffStopsAnswering()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { Ranking = false });

        var home = await client.GetAsync("/");
        var body = await home.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, home.StatusCode);
        Assert.DoesNotContain("href=\"/matches\"", body);

        foreach (var page in new[] { "/matches", "/players", "/patents", "/ranking/all-time", "/ingame", "/api/ranking", "/api/players/patents" })
            Assert.AreEqual(HttpStatusCode.NotFound, (await client.GetAsync(page)).StatusCode, page);
    }

    [TestMethod]
    public async Task TheSkinShopNeedsTheRanking()
    {
        var modules = new ModuleSettings { Ranking = false, SkinShop = true, LiveServer = false, SiteChat = true };

        Assert.IsFalse(modules.IsEnabled(HubModule.SkinShop));
        Assert.IsFalse(modules.IsEnabled(HubModule.SiteChat));

        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task TheAdministrationAreaIsOnlyForAdministrators()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var anonymous = factory.Client();
        using var player = factory.Client(HubWebFactory.Player);
        using var administrator = factory.Client(HubWebFactory.Administrator);

        foreach (var page in new[] { "/admin", "/admin/servers", "/admin/settings", "/admin/modules", "/admin/game-modes", "/admin/campaigns", "/admin/administrators" })
        {
            Assert.AreEqual(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(page)).StatusCode, page);
            Assert.AreEqual(HttpStatusCode.Forbidden, (await player.GetAsync(page)).StatusCode, page);
            Assert.AreEqual(HttpStatusCode.OK, (await administrator.GetAsync(page)).StatusCode, page);
        }
    }

    [TestMethod]
    public async Task AnAdministratorRegistersAServerAndSeesTheTokenOnce()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        var created = await HubWebFactory.PostFormAsync(client, "/admin/servers", "/admin/servers", new Dictionary<string, string> { ["Name"] = "My Server" });
        var location = created.Headers.Location!.OriginalString;
        var first = await client.GetStringAsync(location);
        var second = await client.GetStringAsync(location);

        Assert.AreEqual(HttpStatusCode.Redirect, created.StatusCode);
        Assert.Contains("playstats_token \"my-server:", first);
        Assert.DoesNotContain("YOUR-KEY", first);
        Assert.Contains("YOUR-KEY", second);
    }

    [TestMethod]
    public async Task TheServerPageExplainsThePluginFolderTheConfigurationFileAndTheMatchModes()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        var created = await HubWebFactory.PostFormAsync(client, "/admin/servers", "/admin/servers", new Dictionary<string, string> { ["Name"] = "My Server" });
        var html = await client.GetStringAsync(created.Headers.Location!.OriginalString);

        Assert.Contains("left4dead2 folder", html);
        Assert.Contains("cfg/playstats.cfg", html);
        Assert.Contains("exec playstats.cfg", html);
        Assert.Contains("cfg/sharedplugins.cfg", html);
        Assert.Contains("blob/main/docs/game-server.md", html);
        Assert.DoesNotContain("plugins/README.md", html);

        foreach (var plugin in new[] { "l4d2_playstats_sync", "l4d2_gameinfo_sync", "l4d2_ranking", "l4d2_show_patent_icon", "l4d2_weapon_skins" })
            Assert.Contains($"sm plugins load {plugin}.smx", html, plugin);
    }

    [TestMethod]
    public async Task AFormWithoutTheAntiforgeryTokenIsRefused()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        var response = await client.PostAsync("/admin/servers", new FormUrlEncodedContent(new Dictionary<string, string> { ["Name"] = "My Server" }));
        var servers = await factory.RunAsync(services => services.GetRequiredService<IGameServerService>().ListAsync());

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.IsEmpty(servers);
    }

    [TestMethod]
    public async Task AnAdministratorTurnsModulesOnAndOff()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        var response = await HubWebFactory.PostFormAsync(client, "/admin/modules", "/admin/modules", new Dictionary<string, string>
        {
            ["Ranking"] = "false",
            ["LiveServer"] = "true",
            ["CommunityRanking"] = "true"
        });

        var modules = await factory.RunAsync(services => services.GetRequiredService<ISettingsService>().GetAsync<ModuleSettings>());

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.IsFalse(modules.Ranking);
        Assert.IsTrue(modules.LiveServer);
        Assert.IsTrue(modules.CommunityRanking);
    }

    [TestMethod]
    public async Task TheMixEndpointBalancesEightRankedPlayers()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        var now = RecentNight();
        var first = StatisticsBuilder.Map("c2m1_highway", now, 1, 300, 200);

        await HubWebFactory.UploadAsync(server, first);
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", now.AddMinutes(15), 2, 600, 500));

        var players = first.Survivors('A').Concat(first.Survivors('B')).Select(id => id.ToString()).ToList();

        var response = await server.PostAsJsonAsync("/api/mix", new
        {
            player1 = players[0],
            player2 = players[1],
            player3 = players[2],
            player4 = players[3],
            player5 = players[4],
            player6 = players[5],
            player7 = players[6],
            player8 = players[7]
        });

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var survivors = body.GetProperty("survivors").EnumerateArray().Select(p => p.GetProperty("position").GetInt32()).ToList();
        var infecteds = body.GetProperty("infecteds").EnumerateArray().Select(p => p.GetProperty("position").GetInt32()).ToList();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        CollectionAssert.AreEqual(new[] { 1, 4, 6, 8 }, survivors);
        CollectionAssert.AreEqual(new[] { 2, 3, 5, 7 }, infecteds);
    }

    private static DateTime RecentNight()
    {
        var now = DateTime.UtcNow;

        return new DateTime(now.Year, now.Month, Math.Min(now.Day, 20), 12, 0, 0);
    }
}
