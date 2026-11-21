using System.Net;
using System.Text.Json;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Players;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Community.Support;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class ForcedNamesTests
{
    private const string Page = "/admin/forced-names";
    private const string Forced = "Forced Hero";
    private const string PlayerSteamId = "STEAM_0:0:15152893";

    [TestMethod]
    public async Task AnAdministratorAddsEditsAndRemovesAForcedName()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var administrator = factory.Client(HubWebFactory.Administrator);

        var added = await HubWebFactory.PostFormAsync(administrator, Page, $"{Page}/save", new Dictionary<string, string> { ["communityId"] = PlayerSteamId, ["name"] = $"  {Forced}  ", ["returnUrl"] = Page });
        var list = await administrator.GetStringAsync(Page);
        var edited = await HubWebFactory.PostFormAsync(administrator, Page, $"{Page}/save", new Dictionary<string, string> { ["communityId"] = StatisticsBuilder.PlayerA1.ToString(), ["name"] = "Renamed", ["returnUrl"] = Page });
        var afterEdit = await NamesAsync(factory);
        var removed = await HubWebFactory.PostFormAsync(administrator, Page, $"{Page}/delete", new Dictionary<string, string> { ["communityId"] = StatisticsBuilder.PlayerA1.ToString(), ["returnUrl"] = Page });
        var empty = await administrator.GetStringAsync(Page);

        Assert.AreEqual(HttpStatusCode.Redirect, added.StatusCode);
        Assert.Contains($"<strong>{Forced}</strong>", list);
        Assert.Contains($"data-fn-edit=\"{StatisticsBuilder.PlayerA1}\"", list);
        Assert.Contains("onsubmit=\"return confirm(", list);
        Assert.AreEqual(HttpStatusCode.Redirect, edited.StatusCode);
        Assert.AreEqual("Renamed", afterEdit[StatisticsBuilder.PlayerA1]);
        Assert.AreEqual(HttpStatusCode.Redirect, removed.StatusCode);
        Assert.Contains("The forced name was removed.", empty);
        Assert.IsEmpty(await NamesAsync(factory));
    }

    [TestMethod]
    public async Task AnInvalidPlayerOrAnEmptyNameIsRefused()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var administrator = factory.Client(HubWebFactory.Administrator);

        await HubWebFactory.PostFormAsync(administrator, Page, $"{Page}/save", new Dictionary<string, string> { ["communityId"] = "12", ["name"] = Forced });
        var invalidPlayer = await administrator.GetStringAsync(Page);
        await HubWebFactory.PostFormAsync(administrator, Page, $"{Page}/save", new Dictionary<string, string> { ["communityId"] = PlayerSteamId, ["name"] = "   " });
        var invalidName = await administrator.GetStringAsync(Page);

        Assert.Contains("That is not a valid SteamID64.", invalidPlayer);
        Assert.Contains("Write the name the player must use.", invalidName);
        Assert.IsEmpty(await NamesAsync(factory));
    }

    [TestMethod]
    public async Task RemovingAnInvalidPlayerShowsTheError()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var administrator = factory.Client(HubWebFactory.Administrator);

        var response = await HubWebFactory.PostFormAsync(administrator, Page, $"{Page}/delete", new Dictionary<string, string> { ["communityId"] = "12" });
        var page = await administrator.GetStringAsync(Page);

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("That is not a valid SteamID64.", page);
        Assert.DoesNotContain("The forced name was removed.", page);
    }

    [TestMethod]
    public async Task TheForcedNamesConfigIsImported()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var administrator = factory.Client(HubWebFactory.Administrator);

        await HubWebFactory.PostFormAsync(administrator, Page, $"{Page}/save", new Dictionary<string, string> { ["communityId"] = PlayerSteamId, ["name"] = "Old" });

        const string config = """
                              // Forced Names - l4d2_forced_names.sp
                              "ForcedNames"
                              {
                              	"STEAM_0:0:15152893" "Forced Hero" // https://steamcommunity.com/profiles/76561197990571514
                              	"STEAM_1:1:67149995" "Xei"
                              	"76561198141521946" "Altair"
                              	"STEAM_0:9" "Broken"
                              }
                              """;

        var response = await HubWebFactory.PostFormAsync(administrator, Page, $"{Page}/import", new Dictionary<string, string> { ["text"] = config });
        var page = await administrator.GetStringAsync(Page);
        var names = await NamesAsync(factory);

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("Import finished: 2 added, 1 updated, 0 already up to date, 1 lines left out.", page);
        Assert.AreEqual(Forced, names[StatisticsBuilder.PlayerA1]);
        Assert.AreEqual("Xei", names[76561198094565719]);
        Assert.AreEqual("Altair", names[76561198141521946]);
        Assert.HasCount(3, names);
    }

    [TestMethod]
    public async Task OnlyAnAdministratorManagesTheForcedNames()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var player = factory.Client(HubWebFactory.Player);

        var page = await player.GetAsync(Page);

        await HubWebFactory.PostFormAsync(player, "/", $"{Page}/save", new Dictionary<string, string> { ["communityId"] = PlayerSteamId, ["name"] = Forced });

        Assert.AreNotEqual(HttpStatusCode.OK, page.StatusCode);
        Assert.IsEmpty(await NamesAsync(factory));
    }

    [TestMethod]
    public async Task TheGameServerReadsTheForcedNames()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();

        await SaveAsync(factory, StatisticsBuilder.PlayerA1, Forced);

        using var server = factory.ServerClient(credentials);
        using var visitor = factory.Client();

        var response = await server.GetAsync("/api/forced-names");
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        var anonymous = await visitor.GetAsync("/api/forced-names");
        var entry = body.EnumerateArray().Single();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(StatisticsBuilder.PlayerA1.ToString(), entry.GetProperty("communityId").GetString());
        Assert.AreEqual("STEAM_1:0:15152893", entry.GetProperty("steamId").GetString());
        Assert.AreEqual(Forced, entry.GetProperty("name").GetString());
        Assert.AreEqual(HttpStatusCode.Unauthorized, anonymous.StatusCode);
    }

    [TestMethod]
    public async Task TheSiteShowsTheForcedNameEverywhere()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { Ranking = true });

        var credentials = await factory.CreateServerAsync();

        using (var server = factory.ServerClient(credentials))
        {
            await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", CommunityTestHub.Night, 1, 300, 200));
            await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", CommunityTestHub.Night.AddMinutes(15), 2, 600, 500));
        }

        using var visitor = factory.Client();
        using var administrator = factory.Client(HubWebFactory.Administrator);

        var before = await visitor.GetStringAsync("/?period=all-time");

        await HubWebFactory.PostFormAsync(administrator, Page, $"{Page}/save", new Dictionary<string, string> { ["communityId"] = StatisticsBuilder.PlayerA1.ToString(), ["name"] = Forced });

        var home = await visitor.GetStringAsync("/?period=all-time");
        var player = await visitor.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}?period=all-time");
        var matches = await visitor.GetStringAsync("/matches");
        var search = await administrator.GetStringAsync("/api/admin/players/search?q=forced");

        Assert.DoesNotContain(Forced, before);
        Assert.Contains(Forced, home);
        Assert.Contains($"<title>{Forced}", player);
        Assert.Contains(Forced, matches);
        Assert.Contains(StatisticsBuilder.PlayerA1.ToString(), search);
    }

    private static Task SaveAsync(HubWebFactory factory, long communityId, string name)
    {
        return factory.RunAsync(services => services.GetRequiredService<IForcedNameService>().SaveAsync(communityId, name, null));
    }

    private static Task<Dictionary<long, string>> NamesAsync(HubWebFactory factory)
    {
        return factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Set<ForcedName>().AsNoTracking().ToDictionaryAsync(entry => entry.CommunityId, entry => entry.Name));
    }
}
