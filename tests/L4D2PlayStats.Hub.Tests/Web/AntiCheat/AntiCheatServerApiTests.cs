using System.Net;
using System.Text.Json;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Web.Live;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web.AntiCheat;

[TestClass]
public class AntiCheatServerApiTests
{
    private const string Api = "/api/anti-cheat-access/server";
    private const string Dequeue = "/api/game-info/server-command/dequeue";
    private const string Alice = "76561198000000001";
    private const string AliceSteam2 = "STEAM_1:1:19867136";
    private const long AliceAccount = 39734273;

    [TestMethod]
    public async Task TheSnapshotStartsWithoutAModeAndWithEmptyListsAndOnlyAServerTokenReadsIt()
    {
        using var factory = new LiveWebFactory();

        await StartAsync(factory);

        var credentials = await factory.CreateServerAsync();

        using var server = factory.ServerClient(credentials);
        using var anonymous = factory.Client();
        using var wrong = factory.Client();

        wrong.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "wrong:token");

        var response = await server.GetAsync(Api);
        var json = await response.Content.ReadAsStringAsync();
        var snapshot = JsonSerializer.Deserialize<JsonElement>(json);
        var withoutToken = await anonymous.GetAsync(Api);
        var withWrongToken = await wrong.GetAsync(Api);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("""{"mode":null,"whitelist":[],"required":[]}""", json);
        Assert.AreEqual(JsonValueKind.Null, snapshot.GetProperty("mode").ValueKind);
        Assert.AreEqual(HttpStatusCode.Unauthorized, withoutToken.StatusCode);
        Assert.AreEqual(HttpStatusCode.Unauthorized, withWrongToken.StatusCode);
    }

    [TestMethod]
    public async Task TheGameAddsAndRemovesPlayersWithSteam2IdsAndGetsTheSnapshotBack()
    {
        using var factory = new LiveWebFactory();

        await StartAsync(factory);

        var credentials = await factory.CreateServerAsync();

        using var server = factory.ServerClient(credentials);

        var command = new { player = AliceSteam2, name = "Alice", adminCommunityId = "76561198000000100", adminName = "TheAdmin" };

        var added = await server.PostAsJsonAsync($"{Api}/required", command);
        var addedJson = await added.Content.ReadAsStringAsync();
        var again = await server.PostAsJsonAsync($"{Api}/required", command with { name = "Renamed" });
        var whitelisted = await server.PostAsJsonAsync($"{Api}/whitelist", new { player = "STEAM_0:0:19867137", name = "Bob" });
        var snapshot = await again.Content.ReadFromJsonAsync<JsonElement>();
        var whitelistSnapshot = await whitelisted.Content.ReadFromJsonAsync<JsonElement>();
        var entity = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().AntiCheatRequired.AsNoTracking().SingleAsync());

        Assert.AreEqual(HttpStatusCode.OK, added.StatusCode);
        Assert.AreEqual($$"""{"mode":null,"whitelist":[],"required":[{"accountId":{{AliceAccount}},"communityId":"{{Alice}}","name":"Alice"}]}""", addedJson);
        Assert.AreEqual(HttpStatusCode.OK, again.StatusCode);
        Assert.AreEqual(1, snapshot.GetProperty("required").GetArrayLength());
        Assert.AreEqual("Alice", snapshot.GetProperty("required")[0].GetProperty("name").GetString());
        Assert.AreEqual("76561198000000002", whitelistSnapshot.GetProperty("whitelist")[0].GetProperty("communityId").GetString());
        Assert.AreEqual(AliceAccount + 1, whitelistSnapshot.GetProperty("whitelist")[0].GetProperty("accountId").GetInt64());
        Assert.AreEqual(AntiCheatAccessSource.Game, entity.AddedFrom);
        Assert.AreEqual("TheAdmin", entity.AddedByName);
        Assert.AreEqual(HubWebFactory.Administrator, entity.AddedByCommunityId);

        var removed = await server.DeleteAsync($"{Api}/required?player={AliceSteam2}");
        var removedAgain = await server.DeleteAsync($"{Api}/required?player={AliceSteam2}");
        var afterRemoval = await removed.Content.ReadFromJsonAsync<JsonElement>();

        Assert.AreEqual(HttpStatusCode.OK, removed.StatusCode);
        Assert.AreEqual(0, afterRemoval.GetProperty("required").GetArrayLength());
        Assert.AreEqual(1, afterRemoval.GetProperty("whitelist").GetArrayLength());
        Assert.AreEqual(HttpStatusCode.OK, removedAgain.StatusCode);
    }

    [TestMethod]
    public async Task TheGameChangesTheModeAndTheConsoleIsTheAdminWhenNoNameIsSent()
    {
        using var factory = new LiveWebFactory();

        await StartAsync(factory);

        var credentials = await factory.CreateServerAsync();

        using var server = factory.ServerClient(credentials);

        var response = await server.PutAsJsonAsync($"{Api}/mode", new { mode = "everyone" });
        var snapshot = await response.Content.ReadFromJsonAsync<JsonElement>();
        var setting = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().AntiCheatAccessSettings.AsNoTracking().SingleAsync());

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("everyone", snapshot.GetProperty("mode").GetString());
        Assert.AreEqual(AntiCheatAccessMode.Everyone, setting.Mode);
        Assert.AreEqual("Console", setting.UpdatedByName);
        Assert.IsNull(setting.UpdatedByCommunityId);
        Assert.AreEqual(AntiCheatAccessSource.Game, setting.UpdatedFrom);

        var listed = await server.PutAsJsonAsync($"{Api}/mode", new { mode = "LISTED", adminName = "Ana" });
        var listedSnapshot = await listed.Content.ReadFromJsonAsync<JsonElement>();

        Assert.AreEqual("listed", listedSnapshot.GetProperty("mode").GetString());
    }

    [TestMethod]
    public async Task InvalidModesSteamIdsAndListsAreRefused()
    {
        using var factory = new LiveWebFactory();

        await StartAsync(factory);

        var credentials = await factory.CreateServerAsync();

        using var server = factory.ServerClient(credentials);

        var mode = await server.PutAsJsonAsync($"{Api}/mode", new { mode = "sometimes" });
        var player = await server.PostAsJsonAsync($"{Api}/whitelist", new { player = "alice", name = "Alice" });
        var empty = await server.PostAsJsonAsync($"{Api}/whitelist", new { player = " ", name = "Alice" });
        var list = await server.PostAsJsonAsync($"{Api}/blacklist", new { player = AliceSteam2, name = "Alice" });
        var removePlayer = await server.DeleteAsync($"{Api}/whitelist?player=alice");
        var removeList = await server.DeleteAsync($"{Api}/blacklist?player={AliceSteam2}");
        var modeError = await mode.Content.ReadFromJsonAsync<JsonElement>();
        var playerError = await player.Content.ReadFromJsonAsync<JsonElement>();
        var saved = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().AntiCheatWhitelist.CountAsync());

        Assert.AreEqual(HttpStatusCode.BadRequest, mode.StatusCode);
        Assert.AreEqual("The mode is invalid.", modeError.GetProperty("errorMessage").GetString());
        Assert.AreEqual(HttpStatusCode.BadRequest, player.StatusCode);
        Assert.AreEqual("The Steam ID is invalid.", playerError.GetProperty("errorMessage").GetString());
        Assert.AreEqual(HttpStatusCode.BadRequest, empty.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, list.StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, removePlayer.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, removeList.StatusCode);
        Assert.AreEqual(0, saved);
    }

    [TestMethod]
    public async Task NamesLongerThanTheLimitAreCut()
    {
        using var factory = new LiveWebFactory();

        await StartAsync(factory);

        var credentials = await factory.CreateServerAsync();

        using var server = factory.ServerClient(credentials);

        await server.PostAsJsonAsync($"{Api}/whitelist", new { player = AliceSteam2, name = new string('a', 80), adminName = new string('b', 80) });

        var entity = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().AntiCheatWhitelist.AsNoTracking().SingleAsync());

        Assert.AreEqual(64, entity.Name!.Length);
        Assert.AreEqual(64, entity.AddedByName!.Length);
    }

    [TestMethod]
    public async Task ChangesFromTheGameDoNotQueueTheSyncCommand()
    {
        using var factory = new LiveWebFactory();

        await StartAsync(factory);

        var credentials = await factory.CreateServerAsync();

        using var server = factory.ServerClient(credentials);

        await server.PutAsJsonAsync($"{Api}/mode", new { mode = "everyone" });
        await server.PostAsJsonAsync($"{Api}/whitelist", new { player = AliceSteam2, name = "Alice" });
        await server.DeleteAsync($"{Api}/whitelist?player={AliceSteam2}");

        var command = await server.GetAsync(Dequeue);
        var logs = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Set<RemoteCommandLog>().CountAsync());

        Assert.AreEqual(HttpStatusCode.NoContent, command.StatusCode);
        Assert.AreEqual(0, logs);
    }

    [TestMethod]
    public async Task WithTheAntiCheatModuleOffTheEndpointsAndPagesAreNotFound()
    {
        using var factory = new LiveWebFactory();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { LiveServer = true, AntiCheat = false });

        var credentials = await factory.CreateServerAsync();

        using var server = factory.ServerClient(credentials);
        using var administrator = factory.Client(HubWebFactory.Administrator);

        var snapshot = await server.GetAsync(Api);
        var add = await server.PostAsJsonAsync($"{Api}/whitelist", new { player = AliceSteam2, name = "Alice" });
        var admin = await administrator.GetAsync("/api/anti-cheat-access");
        var page = await administrator.GetAsync("/admin/anti-cheat");
        var publicPage = await administrator.GetAsync("/anti-cheat");

        Assert.AreEqual(HttpStatusCode.NotFound, snapshot.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, add.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, admin.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, page.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, publicPage.StatusCode);
    }

    internal static async Task StartAsync(LiveWebFactory factory, bool remoteCommands = false, bool liveServer = true)
    {
        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { LiveServer = liveServer, RemoteCommands = remoteCommands, AntiCheat = true });
    }
}
