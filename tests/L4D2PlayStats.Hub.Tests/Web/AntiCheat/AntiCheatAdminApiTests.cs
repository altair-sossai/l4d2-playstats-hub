using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Web.Live;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web.AntiCheat;

[TestClass]
public class AntiCheatAdminApiTests
{
    private const string Page = "/admin/anti-cheat";
    private const string Api = "/api/anti-cheat-access";
    private const string Dequeue = "/api/game-info/server-command/dequeue";
    private const string AdminName = "Player 76561198000000100";

    [TestMethod]
    public async Task SiteChangesQueueOneSyncPerGameServerEvenWithRemoteCommandsOff()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await AntiCheatServerApiTests.StartAsync(factory);

        var first = await factory.CreateServerAsync("First Server");
        var second = await factory.CreateServerAsync("Second Server");
        var disabled = await factory.CreateServerAsync("Disabled Server");

        await factory.RunAsync(services => services.GetRequiredService<IGameServerService>().UpdateAsync(disabled.Server.Id, "Disabled Server", null, false));

        using var firstServer = factory.ServerClient(first);
        using var secondServer = factory.ServerClient(second);

        var mode = await LiveWebFactory.SendJsonAsync(client, Page, HttpMethod.Put, $"{Api}/mode", """{"mode":"everyone"}""");
        var add = await LiveWebFactory.SendJsonAsync(client, Page, HttpMethod.Post, $"{Api}/whitelist", """{"player":"https://steamcommunity.com/profiles/76561198000000001/","name":"Alice"}""");
        var logs = await LogsAsync(factory);

        var firstCommand = await firstServer.GetStringAsync(Dequeue);
        var firstAgain = await firstServer.GetAsync(Dequeue);
        var secondCommand = await secondServer.GetStringAsync(Dequeue);
        var secondAgain = await secondServer.GetAsync(Dequeue);
        var delivered = await LogsAsync(factory);

        Assert.AreEqual(HttpStatusCode.OK, mode.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, add.StatusCode);
        Assert.HasCount(2, logs);
        Assert.IsTrue(logs.All(log => log.Command == "sm_aac sync" && log.AdministratorCommunityId == HubWebFactory.Administrator && log.AdministratorName == AdminName));
        Assert.AreSequenceEqual(["First Server", "Second Server"], logs.Select(log => log.GameServerName).Order().ToArray());
        Assert.Contains("\"fullCommand\":\"sm_aac sync\"", firstCommand);
        Assert.AreEqual(HttpStatusCode.NoContent, firstAgain.StatusCode);
        Assert.Contains("\"fullCommand\":\"sm_aac sync\"", secondCommand);
        Assert.AreEqual(HttpStatusCode.NoContent, secondAgain.StatusCode);
        Assert.IsTrue(delivered.All(log => log.DeliveredAtUtc != null));

        var remove = await LiveWebFactory.SendJsonAsync(client, Page, HttpMethod.Delete, $"{Api}/whitelist/76561198000000001", null);
        var afterRemoval = await firstServer.GetStringAsync(Dequeue);

        Assert.AreEqual(HttpStatusCode.NoContent, remove.StatusCode);
        Assert.Contains("\"fullCommand\":\"sm_aac sync\"", afterRemoval);
        Assert.HasCount(4, await LogsAsync(factory));
    }

    [TestMethod]
    public async Task TheSyncGoesFirstAndTheAdministratorCommandsStillFollowWhenRemoteCommandsAreOn()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await AntiCheatServerApiTests.StartAsync(factory, true);

        var credentials = await factory.CreateServerAsync();

        using var server = factory.ServerClient(credentials);

        var fields = new Dictionary<string, string> { ["serverId"] = credentials.Server.Id.ToString(), ["command"] = "!forcestart" };

        await HubWebFactory.PostFormAsync(client, "/admin/remote-commands", "/admin/remote-commands", fields);
        await LiveWebFactory.SendJsonAsync(client, Page, HttpMethod.Put, $"{Api}/mode", """{"mode":"listed"}""");

        var first = await server.GetStringAsync(Dequeue);
        var second = await server.GetStringAsync(Dequeue);
        var third = await server.GetAsync(Dequeue);

        Assert.Contains("\"fullCommand\":\"sm_aac sync\"", first);
        Assert.Contains("\"fullCommand\":\"sm_forcestart\"", second);
        Assert.AreEqual(HttpStatusCode.NoContent, third.StatusCode);
    }

    [TestMethod]
    public async Task WithTheLiveServerModuleOffNothingIsQueued()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await AntiCheatServerApiTests.StartAsync(factory, liveServer: false);
        await factory.CreateServerAsync();

        var mode = await LiveWebFactory.SendJsonAsync(client, Page, HttpMethod.Put, $"{Api}/mode", """{"mode":"everyone"}""");

        Assert.AreEqual(HttpStatusCode.OK, mode.StatusCode);
        Assert.IsEmpty(await LogsAsync(factory));
    }

    [TestMethod]
    public async Task TheAdministratorAddsReadsAndRemovesPlayersOfBothLists()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await AntiCheatServerApiTests.StartAsync(factory);

        var add = await LiveWebFactory.SendJsonAsync(client, Page, HttpMethod.Post, $"{Api}/required", """{"player":"STEAM_1:1:19867136","name":"Alice"}""");
        var duplicate = await LiveWebFactory.SendJsonAsync(client, Page, HttpMethod.Post, $"{Api}/required", """{"player":"76561198000000001","name":"Other"}""");
        var whitelist = await LiveWebFactory.SendJsonAsync(client, Page, HttpMethod.Post, $"{Api}/whitelist", """{"player":"[U:1:39734275]","name":"Carol"}""");
        var invalid = await LiveWebFactory.SendJsonAsync(client, Page, HttpMethod.Post, $"{Api}/required", """{"player":"alice","name":"Alice"}""");
        var empty = await LiveWebFactory.SendJsonAsync(client, Page, HttpMethod.Post, $"{Api}/required", """{"player":"","name":"Alice"}""");
        var unknownList = await LiveWebFactory.SendJsonAsync(client, Page, HttpMethod.Post, $"{Api}/blacklist", """{"player":"76561198000000001","name":"Alice"}""");
        var badMode = await LiveWebFactory.SendJsonAsync(client, Page, HttpMethod.Put, $"{Api}/mode", """{"mode":"nobody"}""");

        var added = await add.Content.ReadFromJsonAsync<JsonElement>();
        var duplicated = await duplicate.Content.ReadFromJsonAsync<JsonElement>();
        var error = await invalid.Content.ReadFromJsonAsync<JsonElement>();
        var emptyError = await empty.Content.ReadFromJsonAsync<JsonElement>();
        var access = await client.GetFromJsonAsync<JsonElement>(Api);

        Assert.AreEqual(HttpStatusCode.OK, add.StatusCode);
        Assert.AreEqual("76561198000000001", added.GetProperty("communityId").GetString());
        Assert.AreEqual(39734273, added.GetProperty("accountId").GetInt64());
        Assert.AreEqual("STEAM_1:1:19867136", added.GetProperty("steamId").GetString());
        Assert.AreEqual("Alice", added.GetProperty("name").GetString());
        Assert.AreEqual(AdminName, added.GetProperty("addedByName").GetString());
        Assert.AreEqual("Site", added.GetProperty("addedFrom").GetString());
        Assert.AreEqual("/avatar/76561198000000001", added.GetProperty("avatarUrl").GetString());
        Assert.AreEqual(HttpStatusCode.OK, duplicate.StatusCode);
        Assert.AreEqual("Alice", duplicated.GetProperty("name").GetString());
        Assert.AreEqual(HttpStatusCode.OK, whitelist.StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.AreEqual("The Steam ID is invalid.", error.GetProperty("errorMessage").GetString());
        Assert.AreEqual(HttpStatusCode.BadRequest, empty.StatusCode);
        Assert.AreEqual("The Steam ID must be provided.", emptyError.GetProperty("errorMessage").GetString());
        Assert.AreEqual(HttpStatusCode.NotFound, unknownList.StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, badMode.StatusCode);
        Assert.AreEqual("listed", access.GetProperty("mode").GetString());
        Assert.IsFalse(access.GetProperty("modeSaved").GetBoolean());
        Assert.AreEqual(1, access.GetProperty("required").GetArrayLength());
        Assert.AreEqual("Carol", access.GetProperty("whitelist")[0].GetProperty("name").GetString());

        var remove = await LiveWebFactory.SendJsonAsync(client, Page, HttpMethod.Delete, $"{Api}/required/76561198000000001", null);
        var required = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().AntiCheatRequired.CountAsync());
        var whitelisted = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().AntiCheatWhitelist.CountAsync());

        Assert.AreEqual(HttpStatusCode.NoContent, remove.StatusCode);
        Assert.AreEqual(0, required);
        Assert.AreEqual(1, whitelisted);
    }

    [TestMethod]
    public async Task TheModeSavedOnTheSiteRecordsWhoSavedItAndFromWhere()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await AntiCheatServerApiTests.StartAsync(factory);

        var response = await LiveWebFactory.SendJsonAsync(client, Page, HttpMethod.Put, $"{Api}/mode", """{"mode":"everyone"}""");
        var access = await response.Content.ReadFromJsonAsync<JsonElement>();
        var setting = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().AntiCheatAccessSettings.AsNoTracking().SingleAsync());

        Assert.AreEqual("everyone", access.GetProperty("mode").GetString());
        Assert.IsTrue(access.GetProperty("modeSaved").GetBoolean());
        Assert.AreEqual("Site", access.GetProperty("updatedFrom").GetString());
        Assert.AreEqual(AntiCheatAccessMode.Everyone, setting.Mode);
        Assert.AreEqual(AntiCheatAccessSource.Site, setting.UpdatedFrom);
        Assert.AreEqual(HubWebFactory.Administrator, setting.UpdatedByCommunityId);
        Assert.AreEqual(AdminName, setting.UpdatedByName);
    }

    [TestMethod]
    public async Task OnlyAnAdministratorWithTheTokenOfThePageChangesTheLists()
    {
        using var factory = new LiveWebFactory();
        using var administrator = factory.Client(HubWebFactory.Administrator);
        using var player = factory.Client(HubWebFactory.Player);
        using var visitor = factory.Client();

        await AntiCheatServerApiTests.StartAsync(factory);

        var command = """{"player":"76561198000000001","name":"Alice"}""";

        var withoutToken = await administrator.PostAsync($"{Api}/whitelist", LiveWebFactory.Json(command));
        var modeWithoutToken = await administrator.PutAsync($"{Api}/mode", LiveWebFactory.Json("""{"mode":"everyone"}"""));
        var asPlayer = await player.PostAsync($"{Api}/whitelist", LiveWebFactory.Json(command));
        var readAsPlayer = await player.GetAsync(Api);
        var pageAsPlayer = await player.GetAsync(Page);
        var lookupAsPlayer = await player.GetAsync("/api/admin/players/lookup?id=76561198000000001");
        var asVisitor = await visitor.GetAsync(Api);
        var saved = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().AntiCheatWhitelist.CountAsync());
        var mode = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().AntiCheatAccessSettings.CountAsync());

        Assert.AreEqual(HttpStatusCode.BadRequest, withoutToken.StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, modeWithoutToken.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, asPlayer.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, readAsPlayer.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, pageAsPlayer.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, lookupAsPlayer.StatusCode);
        Assert.IsFalse(asVisitor.IsSuccessStatusCode);
        Assert.AreEqual(0, saved);
        Assert.AreEqual(0, mode);
    }

    private static Task<List<RemoteCommandLog>> LogsAsync(LiveWebFactory factory)
    {
        return factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Set<RemoteCommandLog>().AsNoTracking().OrderBy(log => log.Id).ToListAsync());
    }
}
