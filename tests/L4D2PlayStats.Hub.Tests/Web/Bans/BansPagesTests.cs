using System.Net;
using System.Net.Http.Json;
using L4D2PlayStats.Hub.Core.Modules.Bans.Enums;
using L4D2PlayStats.Hub.Core.Modules.Bans.Models;
using L4D2PlayStats.Hub.Core.Modules.Bans.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web.Bans;

[TestClass]
public class BansPagesTests
{
    private const string Admin = "/admin/bans";
    private const long Alice = 76561198000000001;
    private const long Alternate = 76561198000000002;
    private const long Stranger = 76561198000000003;

    [TestMethod]
    public async Task ThePagesOfTheBansAnswerNotFoundWhileTheModuleIsOff()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var administrator = factory.Client(HubWebFactory.Administrator);
        using var visitor = factory.Client();

        Assert.AreEqual(HttpStatusCode.NotFound, (await administrator.GetAsync(Admin)).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await visitor.GetAsync("/bans")).StatusCode);
        Assert.DoesNotContain("/admin/bans", await administrator.GetStringAsync("/admin"));
    }

    [TestMethod]
    public async Task AnAdministratorBansEditsAndUnbansAPlayer()
    {
        using var factory = await CreateAsync();
        using var administrator = factory.Client(HubWebFactory.Administrator);

        var banned = await HubWebFactory.PostFormAsync(administrator, Admin, $"{Admin}/save", new Dictionary<string, string> { ["communityId"] = Alice.ToString(), ["name"] = "Alice", ["duration"] = "1440", ["reason"] = "Wallhack", ["returnUrl"] = Admin });
        var list = await administrator.GetStringAsync(Admin);
        var ban = await ActiveAsync(factory, Alice);
        var edited = await HubWebFactory.PostFormAsync(administrator, Admin, $"{Admin}/edit", new Dictionary<string, string> { ["id"] = ban!.Id.ToString(), ["duration"] = "custom", ["endsAt"] = "2099-01-02T03:04", ["reason"] = "Aimbot", ["returnUrl"] = Admin });
        var afterEdit = await ActiveAsync(factory, Alice);
        var lifted = await HubWebFactory.PostFormAsync(administrator, Admin, $"{Admin}/lift", new Dictionary<string, string> { ["id"] = ban.Id.ToString(), ["reason"] = "Mistake", ["returnUrl"] = Admin });
        var afterLift = await administrator.GetStringAsync(Admin);

        Assert.AreEqual(HttpStatusCode.Redirect, banned.StatusCode);
        Assert.Contains("The ban was saved. The game servers apply it at once.", list);
        Assert.Contains("Wallhack", list);
        Assert.Contains($"data-ban-edit=\"{ban.Id}\"", list);
        Assert.Contains("data-confirm=\"", list);
        Assert.AreEqual(HttpStatusCode.Redirect, edited.StatusCode);
        Assert.AreEqual("Aimbot", afterEdit!.Reason);
        Assert.AreEqual(new DateTime(2099, 1, 2, 3, 4, 0, DateTimeKind.Utc), afterEdit.EndsAtUtc);
        Assert.AreEqual(HttpStatusCode.Redirect, lifted.StatusCode);
        Assert.Contains("The ban was lifted. The player can join the game servers again.", afterLift);
        Assert.Contains("Mistake", afterLift);
        Assert.IsNull(await ActiveAsync(factory, Alice));
    }

    [TestMethod]
    public async Task AWrongPlayerAnEndInThePastOrASecondBanAreRefused()
    {
        using var factory = await CreateAsync();
        using var administrator = factory.Client(HubWebFactory.Administrator);

        await HubWebFactory.PostFormAsync(administrator, Admin, $"{Admin}/save", new Dictionary<string, string> { ["communityId"] = "12", ["duration"] = "0" });
        var invalidPlayer = await administrator.GetStringAsync(Admin);
        await HubWebFactory.PostFormAsync(administrator, Admin, $"{Admin}/save", new Dictionary<string, string> { ["communityId"] = Alice.ToString(), ["duration"] = "custom", ["endsAt"] = "2001-01-01T00:00" });
        var past = await administrator.GetStringAsync(Admin);
        await HubWebFactory.PostFormAsync(administrator, Admin, $"{Admin}/save", new Dictionary<string, string> { ["communityId"] = Alice.ToString(), ["duration"] = "7" });
        var unknownDuration = await administrator.GetStringAsync(Admin);
        await HubWebFactory.PostFormAsync(administrator, Admin, $"{Admin}/save", new Dictionary<string, string> { ["communityId"] = Alice.ToString(), ["duration"] = "0" });
        await HubWebFactory.PostFormAsync(administrator, Admin, $"{Admin}/save", new Dictionary<string, string> { ["communityId"] = Alice.ToString(), ["duration"] = "0" });
        var twice = await administrator.GetStringAsync(Admin);

        Assert.Contains("That is not a valid SteamID64.", invalidPlayer);
        Assert.Contains("Choose when the ban ends: a date in the future, or permanent.", past);
        Assert.Contains("Choose when the ban ends: a date in the future, or permanent.", unknownDuration);
        Assert.Contains("This player is already banned. Edit the ban on the list instead.", twice);
    }

    [TestMethod]
    public async Task BanningAlsoBansTheRelatedAccountsTheAdministratorKept()
    {
        using var factory = await CreateAsync();
        using var administrator = factory.Client(HubWebFactory.Administrator);

        await ConnectAsync(factory, Alice, "10.0.0.1");
        await ConnectAsync(factory, Alternate, "10.0.0.1");
        await ConnectAsync(factory, Stranger, "10.0.0.9");

        var response = await HubWebFactory.PostFormAsync(administrator, Admin, $"{Admin}/save", new Dictionary<string, string> { ["communityId"] = Alice.ToString(), ["duration"] = "0", ["reason"] = "Cheating", ["relatedIds"] = Alternate.ToString(), ["returnUrl"] = Admin });
        var related = await ActiveAsync(factory, Alternate);
        var main = await ActiveAsync(factory, Alice);
        var page = await administrator.GetStringAsync(Admin);
        var lookup = await administrator.GetStringAsync($"/api/admin/players/related?id={Alice}&list=bans");

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual("Cheating", related!.Reason);
        Assert.AreEqual(main!.Id, related.RelatedToBanId);
        Assert.Contains("Related accounts included: 1.", page);
        Assert.Contains("\"linked\":true", lookup);
        Assert.IsNull(await ActiveAsync(factory, Stranger));
    }

    [TestMethod]
    public async Task TheAlertsTabListsTheNewLinkedAccountsAndAnAlertCanBeIgnored()
    {
        using var factory = await CreateAsync();
        using var administrator = factory.Client(HubWebFactory.Administrator);

        await ConnectAsync(factory, Alice, "10.0.0.1");
        await HubWebFactory.PostFormAsync(administrator, Admin, $"{Admin}/save", new Dictionary<string, string> { ["communityId"] = Alice.ToString(), ["name"] = "Alice", ["duration"] = "0", ["reason"] = "Wallhack" });

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        await Task.Delay(10);
        var connected = await server.PostAsJsonAsync("/api/game-info/player-connection-info", new { communityId = Alternate.ToString(), ipAddress = "10.0.0.1", name = "Alt" });
        var page = await administrator.GetStringAsync($"{Admin}?tab=alerts");
        var alert = (await factory.RunAsync(services => services.GetRequiredService<IBanAlertService>().PageAsync(1, 20))).Items.Single();
        var ignored = await HubWebFactory.PostFormAsync(administrator, Admin, $"{Admin}/alerts/ignore", new Dictionary<string, string> { ["id"] = alert.Alert.Id.ToString(), ["returnUrl"] = $"{Admin}?tab=alerts" });
        var afterIgnore = await administrator.GetStringAsync($"{Admin}?tab=alerts");
        var bell = await administrator.GetStringAsync("/notifications");

        Assert.AreEqual(HttpStatusCode.OK, connected.StatusCode);
        Assert.Contains($"data-alert-id=\"{alert.Alert.Id}\"", page);
        Assert.Contains($"data-ban-related-to=\"{alert.Ban.Id}\"", page);
        Assert.Contains("Related account of Player 1", page);
        Assert.AreEqual(HttpStatusCode.Redirect, ignored.StatusCode);
        Assert.Contains("The alert was ignored. It is not shown again for this ban.", afterIgnore);
        Assert.DoesNotContain($"data-alert-id=\"{alert.Alert.Id}\"", afterIgnore);
        Assert.Contains("shares an IP address with", bell);
    }

    [TestMethod]
    public async Task TheImportReadsTheBannedUserCfgOfAServer()
    {
        using var factory = await CreateAsync();
        using var administrator = factory.Client(HubWebFactory.Administrator);

        var response = await HubWebFactory.PostFormAsync(administrator, Admin, $"{Admin}/import", new Dictionary<string, string> { ["text"] = "// Name: Alice\n// Ban reason: Cheating\nbanid 0 STEAM_1:1:19867136\nbanid 0 broken" });
        var page = await administrator.GetStringAsync(Admin);
        var ban = await ActiveAsync(factory, Alice);

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("Import finished: 1 added, 0 already banned or already over, 1 lines left out.", page);
        Assert.AreEqual(BanSource.Import, ban!.Source);
        Assert.AreEqual("Cheating", ban.Reason);
    }

    [TestMethod]
    public async Task ThePublicPageListsTheActiveBansAndThePlayerPageShowsTheBan()
    {
        using var factory = await CreateAsync();

        await AddAsync(factory, Alice, null, "Wallhack", "Alice");
        await AddAsync(factory, Alternate, DateTime.UtcNow.AddDays(2), "Rage quit", "Bob");

        var lifted = await ActiveAsync(factory, Alternate);
        await factory.RunAsync(services => services.GetRequiredService<IBanService>().LiftAsync(lifted!.Id, null, new BanActor(HubWebFactory.Administrator, "Admin")));
        await AddAsync(factory, Stranger, DateTime.UtcNow.AddDays(2), "Spam", "Carol");

        using var visitor = factory.Client();

        var list = await visitor.GetStringAsync("/bans");
        var search = await visitor.GetStringAsync("/bans?q=spam");
        var player = await visitor.GetAsync($"/player/{Alice}");
        var html = await player.Content.ReadAsStringAsync();
        var free = await (await visitor.GetAsync($"/player/{Alternate}")).Content.ReadAsStringAsync();

        Assert.Contains("Wallhack", list);
        Assert.Contains("Spam", list);
        Assert.DoesNotContain("Rage quit", list);
        Assert.DoesNotContain("Wallhack", search);
        Assert.Contains("Alice is banned from the game servers permanently.", html);
        Assert.Contains("Reason: Wallhack", html);
        Assert.DoesNotContain("player-ban", free);
        Assert.DoesNotContain("10.0.0", list);
    }

    private static async Task<HubWebFactory> CreateAsync()
    {
        var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { Ranking = true, LiveServer = true, Bans = true });

        return factory;
    }

    private static Task<L4D2PlayStats.Hub.Core.Modules.Bans.Ban?> ActiveAsync(HubWebFactory factory, long communityId)
    {
        return factory.RunAsync(services => services.GetRequiredService<IBanService>().ActiveAsync(communityId));
    }

    private static Task<BanSaveResult> AddAsync(HubWebFactory factory, long communityId, DateTime? endsAt, string reason, string name)
    {
        return factory.RunAsync(services => services.GetRequiredService<IBanService>().AddAsync(new BanRequest(communityId, name, endsAt, reason), new BanActor(HubWebFactory.Administrator, "Admin")));
    }

    private static Task ConnectAsync(HubWebFactory factory, long communityId, string address)
    {
        return factory.RunAsync(services => services.GetRequiredService<IPlayerConnectionService>().RegisterAsync(new PlayerConnectionCommand { CommunityId = communityId.ToString(), IpAddress = address, Name = $"Player {communityId % 100}" }));
    }
}
