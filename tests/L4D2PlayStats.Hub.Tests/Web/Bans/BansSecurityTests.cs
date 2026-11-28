using System.Net;
using System.Net.Http.Json;
using L4D2PlayStats.Hub.Core.Modules.Bans.Models;
using L4D2PlayStats.Hub.Core.Modules.Bans.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web.Bans;

[TestClass]
public class BansSecurityTests
{
    private const string Admin = "/admin/bans";
    private const long Target = 76561198000000001;
    private const long Alternate = 76561198000000002;
    private const string Script = "<script>alert(1)</script>";

    [TestMethod]
    public async Task APlayerCannotBanUnbanOrSeeTheAlertsOfTheAdministration()
    {
        using var factory = await CreateAsync();
        using var player = factory.Client(HubWebFactory.Player);

        await factory.RunAsync(services => services.GetRequiredService<IBanService>().AddAsync(new BanRequest(Target, "Target", null, "Cheating"), new BanActor(HubWebFactory.Administrator, "Admin")));

        var ban = await ActiveAsync(factory, Target);
        var token = HubWebFactory.Token(await player.GetStringAsync("/notifications"));
        var fields = new Dictionary<string, string> { [HubWebFactory.TokenField] = token, ["id"] = ban!.Id.ToString(), ["communityId"] = Alternate.ToString(), ["duration"] = "0", ["text"] = $"banid 0 {Alternate}" };

        foreach (var action in new[] { "save", "edit", "lift", "import", "alerts/ignore" })
        {
            var response = await player.PostAsync($"{Admin}/{action}", new FormUrlEncodedContent(fields));

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, action);
        }

        var related = await player.GetAsync($"/api/admin/players/related?id={Target}&list=bans");
        var serverApi = await player.PostAsJsonAsync("/api/bans", new { player = Alternate.ToString(), minutes = 0 });

        Assert.AreEqual(HttpStatusCode.Forbidden, related.StatusCode);
        Assert.AreEqual(HttpStatusCode.Unauthorized, serverApi.StatusCode);
        Assert.IsNotNull(await ActiveAsync(factory, Target));
        Assert.IsNull(await ActiveAsync(factory, Alternate));
    }

    [TestMethod]
    public async Task TheTextSentByAGameServerIsEncodedOnEveryPage()
    {
        using var factory = await CreateAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);
        using var administrator = factory.Client(HubWebFactory.Administrator);
        using var visitor = factory.Client();

        await server.PostAsJsonAsync("/api/bans", new { player = Target.ToString(), name = Script, minutes = 0, reason = Script, command = Script, adminName = Script });

        foreach (var page in new[] { "/bans", $"/player/{Target}", Admin })
        {
            var html = await (page == Admin ? administrator : visitor).GetStringAsync(page);

            Assert.DoesNotContain(Script, html, page);
            Assert.Contains("&lt;script&gt;", html, page);
        }
    }

    [TestMethod]
    public async Task TheFormsOfTheBansNeverRedirectOutsideTheSite()
    {
        using var factory = await CreateAsync();
        using var administrator = factory.Client(HubWebFactory.Administrator);

        var response = await HubWebFactory.PostFormAsync(administrator, Admin, $"{Admin}/save", new Dictionary<string, string> { ["communityId"] = Target.ToString(), ["duration"] = "0", ["returnUrl"] = "https://evil.example.com/" });
        var protocolRelative = await HubWebFactory.PostFormAsync(administrator, Admin, $"{Admin}/lift", new Dictionary<string, string> { ["id"] = "999", ["returnUrl"] = "//evil.example.com/" });

        Assert.AreEqual(Admin, response.Headers.Location!.OriginalString);
        Assert.AreEqual(Admin, protocolRelative.Headers.Location!.OriginalString);
    }

    [TestMethod]
    public async Task TheIpAddressesAndTheAlertsNeverReachThePublicPages()
    {
        using var factory = await CreateAsync();
        using var visitor = factory.Client();
        using var player = factory.Client(HubWebFactory.Player);

        await ConnectAsync(factory, Target, "203.0.113.55");
        await factory.RunAsync(services => services.GetRequiredService<IBanService>().CaptureAsync(new GameBanCommand { Player = Target.ToString(), Minutes = 0, Reason = "Cheating", Command = "sm_ban" }, "Server"));
        await Task.Delay(10);
        await ConnectAsync(factory, Alternate, "203.0.113.55");
        await factory.RunAsync(services => services.GetRequiredService<IBanAlertService>().DetectAsync(Alternate));

        foreach (var (client, page) in new[] { (visitor, "/bans"), (visitor, $"/player/{Target}"), (player, "/bans"), (player, "/notifications") })
        {
            var html = await (await client.GetAsync(page)).Content.ReadAsStringAsync();

            Assert.DoesNotContain("203.0.113.55", html, page);
            Assert.DoesNotContain("shares an IP address", html, page);
            Assert.DoesNotContain("data-alert-id", html, page);
        }
    }

    [TestMethod]
    public async Task AGameServerCannotLiftOrCreateBansWithoutAValidPlayer()
    {
        using var factory = await CreateAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        var negative = await server.PostAsJsonAsync("/api/bans", new { player = Target.ToString(), minutes = -5 });
        var tooShort = await server.PostAsJsonAsync("/api/bans", new { player = "76561197960265728", minutes = 0 });
        var huge = await server.PostAsJsonAsync("/api/bans", new { player = Target.ToString(), minutes = int.MaxValue, reason = new string('x', 5000) });
        var ban = await ActiveAsync(factory, Target);

        Assert.AreEqual(HttpStatusCode.BadRequest, negative.StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, tooShort.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, huge.StatusCode);
        Assert.AreEqual(300, ban!.Reason!.Length);
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

    private static Task ConnectAsync(HubWebFactory factory, long communityId, string address)
    {
        return factory.RunAsync(services => services.GetRequiredService<IPlayerConnectionService>().RegisterAsync(new PlayerConnectionCommand { CommunityId = communityId.ToString(), IpAddress = address, Name = $"Player {communityId % 100}" }));
    }
}
