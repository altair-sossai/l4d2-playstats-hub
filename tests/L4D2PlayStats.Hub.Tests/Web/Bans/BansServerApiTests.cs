using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using L4D2PlayStats.Hub.Core.Modules.Bans.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web.Bans;

[TestClass]
public class BansServerApiTests
{
    private const long Alice = 76561198000000001;
    private const long Admin = 76561198000000100;

    [TestMethod]
    public async Task ABanOfTheGameServerIsSavedAndTheServerGetsTheStatusOfThePlayer()
    {
        using var factory = await CreateAsync();

        var credentials = await factory.CreateServerAsync("Zone #1");
        using var server = factory.ServerClient(credentials);

        var created = await server.PostAsJsonAsync("/api/bans", new { player = "STEAM_1:1:19867136", name = "Alice", minutes = 90, reason = "Rage quit", command = "sm_ban", adminCommunityId = Admin.ToString(), adminName = "Admin" });
        var answer = await JsonAsync(created);
        var status = await JsonAsync(await server.GetAsync($"/api/bans/{Alice}"));
        var ban = await factory.RunAsync(services => services.GetRequiredService<IBanService>().ActiveAsync(Alice));

        Assert.AreEqual(HttpStatusCode.OK, created.StatusCode);
        Assert.IsTrue(answer.GetProperty("banned").GetBoolean());
        Assert.AreEqual(Alice.ToString(), answer.GetProperty("communityId").GetString());
        Assert.AreEqual("STEAM_1:1:19867136", answer.GetProperty("steamId").GetString());
        Assert.AreEqual("STEAM_0:1:19867136", answer.GetProperty("legacySteamId").GetString());
        Assert.AreEqual("Rage quit", status.GetProperty("reason").GetString());
        Assert.IsFalse(status.GetProperty("permanent").GetBoolean());
        Assert.AreEqual(90, status.GetProperty("minutesLeft").GetInt32());
        Assert.IsFalse(string.IsNullOrEmpty(status.GetProperty("endsAt").GetString()));
        Assert.AreEqual("Zone #1", ban!.GameServerName);
        Assert.AreEqual(Admin, ban.CreatedByCommunityId);
    }

    [TestMethod]
    public async Task EveryConnectionOfABannedPlayerIsCountedAndAnUnbanLetsThePlayerIn()
    {
        using var factory = await CreateAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        await server.PostAsJsonAsync("/api/bans", new { player = Alice.ToString(), minutes = 0, reason = "Cheating", command = "sm_addban" });

        var connection = await JsonAsync(await server.PostAsJsonAsync("/api/bans/connections", new { player = Alice.ToString() }));
        var lifted = await JsonAsync(await server.PostAsJsonAsync("/api/bans/lift", new { player = "STEAM_0:1:19867136", command = "sm_unban" }));
        var afterLift = await JsonAsync(await server.PostAsJsonAsync("/api/bans/connections", new { player = Alice.ToString() }));
        var history = await factory.RunAsync(services => services.GetRequiredService<IBanService>().HistoryAsync(Alice));

        Assert.IsTrue(connection.GetProperty("banned").GetBoolean());
        Assert.IsTrue(connection.GetProperty("permanent").GetBoolean());
        Assert.AreEqual(0, connection.GetProperty("minutesLeft").GetInt32());
        Assert.IsFalse(lifted.GetProperty("banned").GetBoolean());
        Assert.IsFalse(afterLift.GetProperty("banned").GetBoolean());
        Assert.AreEqual(1, history.Single().Attempts);
        Assert.IsNotNull(history.Single().LiftedAtUtc);
    }

    [TestMethod]
    public async Task AnInvalidPlayerIsABadRequest()
    {
        using var factory = await CreateAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        Assert.AreEqual(HttpStatusCode.BadRequest, (await server.PostAsJsonAsync("/api/bans", new { player = "nobody", minutes = 0 })).StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, (await server.PostAsJsonAsync("/api/bans/connections", new { player = "" })).StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, (await server.PostAsJsonAsync("/api/bans/lift", new { })).StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, (await server.GetAsync("/api/bans/abc")).StatusCode);
    }

    [TestMethod]
    public async Task TheBanEndpointsNeedTheTokenOfAServerAndTheModule()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);
        using var anonymous = factory.Client();

        var withoutModule = await server.GetAsync($"/api/bans/{Alice}");

        await factory.SaveAsync(new ModuleSettings { Ranking = true, LiveServer = true, Bans = true });

        var withoutToken = await anonymous.GetAsync($"/api/bans/{Alice}");
        var withToken = await server.GetAsync($"/api/bans/{Alice}");

        Assert.AreEqual(HttpStatusCode.NotFound, withoutModule.StatusCode);
        Assert.AreEqual(HttpStatusCode.Unauthorized, withoutToken.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, withToken.StatusCode);
    }

    [TestMethod]
    public async Task ABanGivenOnTheSiteSendsTheSyncCommandToTheServers()
    {
        using var factory = await CreateAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);
        using var administrator = factory.Client(HubWebFactory.Administrator);

        await HubWebFactory.PostFormAsync(administrator, "/admin/bans", "/admin/bans/save", new Dictionary<string, string> { ["communityId"] = Alice.ToString(), ["duration"] = "60" });

        var command = await JsonAsync(await server.GetAsync("/api/game-info/server-command/dequeue"));

        Assert.AreEqual($"sm_playstats_ban_sync {Alice}", command.GetProperty("fullCommand").GetString());
    }

    [TestMethod]
    public async Task TheConnectionOfALinkedAccountToABannedOneRaisesAnAlert()
    {
        using var factory = await CreateAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        await server.PostAsJsonAsync("/api/game-info/player-connection-info", new { communityId = Alice.ToString(), ipAddress = "10.0.0.1", name = "Alice" });
        await server.PostAsJsonAsync("/api/bans", new { player = Alice.ToString(), minutes = 0, command = "sm_ban" });
        await Task.Delay(10);

        var related = await server.PostAsJsonAsync("/api/game-info/player-connection-info", new { communityId = (Alice + 1).ToString(), ipAddress = "10.0.0.1", name = "Alt" });
        var body = await related.Content.ReadAsStringAsync();
        var alerts = await factory.RunAsync(services => services.GetRequiredService<IBanAlertService>().OpenCountAsync());

        Assert.AreEqual(HttpStatusCode.OK, related.StatusCode);
        Assert.StartsWith("[", body);
        Assert.AreEqual(1, alerts);
    }

    private static async Task<HubWebFactory> CreateAsync()
    {
        var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { Ranking = true, LiveServer = true, Bans = true });

        return factory;
    }

    private static async Task<JsonElement> JsonAsync(HttpResponseMessage response)
    {
        Assert.IsTrue(response.IsSuccessStatusCode, $"{response.RequestMessage?.RequestUri} answered {response.StatusCode}");

        return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
    }
}
