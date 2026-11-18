using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Enums;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Models;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Services;
using L4D2PlayStats.Hub.Core.Modules.Players;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Web.Live;
using L4D2PlayStats.Hub.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web.AntiCheat;

[TestClass]
public class AntiCheatPagesTests
{
    private const string Page = "/admin/anti-cheat";
    private const string PublicPage = "/anti-cheat";
    private const long Alice = 76561198000000001;
    private const long Bob = 76561198000000002;
    private const string Required = "Your Steam account must have the Anti-Cheat running to play on our servers.";
    private const string NotRequired = "Your Steam account can play on our servers without the Anti-Cheat.";

    [TestMethod]
    public async Task BeforeAnythingIsSavedThePageShowsTheRequiredListAndTheConfigFileMode()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await AntiCheatServerApiTests.StartAsync(factory);

        var page = WebUtility.HtmlDecode(await client.GetStringAsync(Page));

        Assert.Contains("<h3>Anti-Cheat settings</h3>", page);
        Assert.Contains("Not saved on the site yet: the server is using the anti_cheat_mode value from its config file.", page);
        Assert.Contains("value=\"listed\" checked", page);
        Assert.DoesNotContain("value=\"everyone\" checked", page);
        Assert.Contains("<h4>Only these players</h4>", page);
        Assert.Contains("See the whitelist (0)", page);
        Assert.Contains("No players on this list.", page);
        Assert.Contains("Anti-Cheat service address", page);
        Assert.Contains("id=\"aca-add-modal\"", page);
        Assert.Contains("id=\"aca-details-modal\"", page);
        Assert.Contains("id=\"pp-modal\"", page);
        Assert.Contains("/api/admin/players/lookup", page);
        Assert.DoesNotContain("This list is not used in the current mode.", page);
    }

    [TestMethod]
    public async Task ThePageShowsTheSavedModeAndThePlayersOfEachList()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await AntiCheatServerApiTests.StartAsync(factory);

        await factory.RunAsync(async services =>
        {
            var access = services.GetRequiredService<IAntiCheatAccessService>();
            var game = new AntiCheatAccessActor(null, null);

            await access.SetModeAsync(AntiCheatAccessMode.Everyone, game, AntiCheatAccessSource.Game);
            await access.AddAsync(AntiCheatAccessList.Whitelist, Alice, "AliceTheExempt", new AntiCheatAccessActor(HubWebFactory.Administrator, "TheAdmin"), AntiCheatAccessSource.Site);
            await access.AddAsync(AntiCheatAccessList.Whitelist, Bob, "BobTheExempt", game, AntiCheatAccessSource.Game);
            await access.AddAsync(AntiCheatAccessList.Required, Bob + 1, "CarolTheRequired", game, AntiCheatAccessSource.Game);
        });

        var page = WebUtility.HtmlDecode(await client.GetStringAsync(Page));
        var required = WebUtility.HtmlDecode(await client.GetStringAsync($"{Page}?list=required"));
        var search = WebUtility.HtmlDecode(await client.GetStringAsync($"{Page}?list=whitelist&q=bobthe"));

        Assert.Contains("value=\"everyone\" checked", page);
        Assert.Contains("Last changed by Console in the game on", page);
        Assert.Contains("<h4>Except these players</h4>", page);
        Assert.Contains("See the required list (1)", page);
        Assert.Contains("data-community-id=\"76561198000000001\"", page);
        Assert.Contains("data-name=\"AliceTheExempt\"", page);
        Assert.Contains("data-steam-id=\"STEAM_1:1:19867136\"", page);
        Assert.Contains("data-profile-url=\"https://steamcommunity.com/profiles/76561198000000001\"", page);
        Assert.Contains("data-avatar=\"/avatar/76561198000000001\"", page);
        Assert.Contains("by TheAdmin on the site", page);
        Assert.Contains("by Console in the game", page);
        Assert.Contains("BobTheExempt", page);
        Assert.DoesNotContain("CarolTheRequired", page);
        Assert.Contains("<h4>Only these players</h4>", required);
        Assert.Contains("CarolTheRequired", required);
        Assert.Contains("This list is not used in the current mode. It stays saved for when the mode changes.", required);
        Assert.Contains("See the whitelist (2)", required);
        Assert.Contains("BobTheExempt", search);
        Assert.DoesNotContain("AliceTheExempt", search);
        Assert.Contains("Clear filters", search);
    }

    [TestMethod]
    public async Task TheListsHaveTwentyPlayersPerPage()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await AntiCheatServerApiTests.StartAsync(factory);

        await factory.RunAsync(async services =>
        {
            var access = services.GetRequiredService<IAntiCheatAccessService>();

            for (var index = 0; index < 25; index++)
            {
                factory.Clock.Advance(TimeSpan.FromMinutes(1));

                await access.AddAsync(AntiCheatAccessList.Required, Alice + index, $"Listed{index:00}Player", new AntiCheatAccessActor(null, null), AntiCheatAccessSource.Game);
            }
        });

        var first = WebUtility.HtmlDecode(await client.GetStringAsync(Page));
        var second = WebUtility.HtmlDecode(await client.GetStringAsync($"{Page}?list=required&page=2"));

        Assert.Contains("Showing 1–20 of 25", first);
        Assert.Contains("Listed24Player", first);
        Assert.DoesNotContain("Listed04Player", first);
        Assert.Contains("Showing 21–25 of 25", second);
        Assert.Contains("Listed04Player", second);
        Assert.Contains("/admin/anti-cheat?list=required&page=2", first);
    }

    [TestMethod]
    public async Task TheAntiCheatSettingsAreTheItemOfTheAdministrationMenu()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await AntiCheatServerApiTests.StartAsync(factory);

        var page = WebUtility.HtmlDecode(await client.GetStringAsync("/admin"));

        Assert.Contains("Anti-Cheat settings", page);
        Assert.DoesNotContain(">Anti-Cheat page<", page);
    }

    [TestMethod]
    [DataRow(AntiCheatAccessMode.Listed, AntiCheatAccessList.Required, true)]
    [DataRow(AntiCheatAccessMode.Listed, AntiCheatAccessList.Whitelist, false)]
    [DataRow(AntiCheatAccessMode.Everyone, AntiCheatAccessList.Whitelist, false)]
    [DataRow(AntiCheatAccessMode.Everyone, AntiCheatAccessList.Required, true)]
    public async Task ASignedInPlayerReadsWhetherTheirAccountMustUseTheAntiCheat(AntiCheatAccessMode mode, AntiCheatAccessList list, bool listedMustUseIt)
    {
        using var factory = new LiveWebFactory();
        using var listed = factory.Client(HubWebFactory.Player);
        using var unlisted = factory.Client(HubWebFactory.Player + 1);
        using var visitor = factory.Client();

        await AntiCheatServerApiTests.StartAsync(factory);

        await factory.RunAsync(async services =>
        {
            var access = services.GetRequiredService<IAntiCheatAccessService>();
            var game = new AntiCheatAccessActor(null, null);

            await access.SetModeAsync(mode, game, AntiCheatAccessSource.Game);
            await access.AddAsync(list, HubWebFactory.Player, "Listed", game, AntiCheatAccessSource.Game);
        });

        var listedPage = WebUtility.HtmlDecode(await listed.GetStringAsync(PublicPage));
        var unlistedPage = WebUtility.HtmlDecode(await unlisted.GetStringAsync(PublicPage));
        var visitorPage = WebUtility.HtmlDecode(await visitor.GetStringAsync(PublicPage));
        var unlistedMustUseIt = mode == AntiCheatAccessMode.Everyone;

        Assert.Contains(listedMustUseIt ? Required : NotRequired, listedPage);
        Assert.DoesNotContain(listedMustUseIt ? NotRequired : Required, listedPage);
        Assert.Contains(unlistedMustUseIt ? Required : NotRequired, unlistedPage);
        Assert.DoesNotContain(unlistedMustUseIt ? NotRequired : Required, unlistedPage);
        Assert.DoesNotContain(Required, visitorPage);
        Assert.DoesNotContain(NotRequired, visitorPage);
    }

    [TestMethod]
    public async Task TheLookupNamesAPlayerOfTheSiteWithoutAskingSteam()
    {
        using var factory = new LiveWebFactory();

        var calls = 0;

        using var application = WithSteam(factory, _ =>
        {
            calls++;

            return Xml("Steam Name");
        });

        await AntiCheatServerApiTests.StartAsync(factory);
        await factory.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();

            dbContext.PlayerProfiles.Add(new PlayerProfile { CommunityId = Alice, Name = "AliceOnTheSite", FirstSeenAtUtc = DateTime.UtcNow, LastSeenAtUtc = DateTime.UtcNow });
            await dbContext.SaveChangesAsync();
        });

        using var client = AdministratorClient(application);

        var player = await client.GetFromJsonAsync<JsonElement>("/api/admin/players/lookup?id=STEAM_1:1:19867136");
        var invalid = await client.GetAsync("/api/admin/players/lookup?id=alice");

        Assert.AreEqual("76561198000000001", player.GetProperty("communityId").GetString());
        Assert.AreEqual("STEAM_1:1:19867136", player.GetProperty("steamId").GetString());
        Assert.AreEqual("[U:1:39734273]", player.GetProperty("steam3").GetString());
        Assert.AreEqual("https://steamcommunity.com/profiles/76561198000000001", player.GetProperty("profileUrl").GetString());
        Assert.AreEqual("AliceOnTheSite", player.GetProperty("name").GetString());
        Assert.AreEqual("/avatar/76561198000000001", player.GetProperty("avatarUrl").GetString());
        Assert.IsTrue(player.GetProperty("knownOnSite").GetBoolean());
        Assert.AreEqual(HttpStatusCode.NotFound, invalid.StatusCode);
        Assert.AreEqual(0, calls);
    }

    [TestMethod]
    public async Task TheLookupAsksSteamForAPlayerWhoNeverPlayedHere()
    {
        using var factory = new LiveWebFactory();

        var calls = 0;

        using var application = WithSteam(factory, _ =>
        {
            calls++;

            return Xml("Steam Stranger");
        });

        await AntiCheatServerApiTests.StartAsync(factory);

        using var client = AdministratorClient(application);

        var player = await client.GetFromJsonAsync<JsonElement>("/api/admin/players/lookup?id=https://steamcommunity.com/profiles/76561198000000009");
        var again = await client.GetFromJsonAsync<JsonElement>("/api/admin/players/lookup?id=76561198000000009");

        Assert.AreEqual("Steam Stranger", player.GetProperty("name").GetString());
        Assert.IsFalse(player.GetProperty("knownOnSite").GetBoolean());
        Assert.AreEqual("Steam Stranger", again.GetProperty("name").GetString());
        Assert.AreEqual(1, calls);
    }

    [TestMethod]
    public async Task TheChatBanFormPreviewsThePlayerOfTheTypedSteamId()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();

        var page = await client.GetStringAsync("/admin/chat-bans");

        Assert.Contains("var lookupUrl = \"/api/admin/players/lookup\";", page);
        Assert.Contains("$.getJSON(lookupUrl, { id: value })", page);
    }

    private static WebApplicationFactory<Program> WithSteam(HubWebFactory factory, Func<Uri, HttpResponseMessage> route)
    {
        var handler = new RoutingHttpMessageHandler(route);

        return factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddHttpClient(AppConsts.SteamHttpClient).ConfigurePrimaryHttpMessageHandler(() => handler)));
    }

    private static HttpClient AdministratorClient(WebApplicationFactory<Program> application)
    {
        var client = application.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        client.DefaultRequestHeaders.Add(HubWebFactory.UserHeader, HubWebFactory.Administrator.ToString());

        return client;
    }

    private static HttpResponseMessage Xml(string name)
    {
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent($"<profile><steamID><![CDATA[{name}]]></steamID></profile>") };
    }
}
