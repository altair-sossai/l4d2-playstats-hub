using System.Net;
using System.Text.Json;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Players;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Web.Live;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class PlayerSearchTests
{
    private const string Api = "/api/admin/players/search";
    private const long Alice = 76561198000000001;
    private const long Bob = 76561198000000002;
    private const long Carol = 76561198000000003;
    private const long Stranger = 76561198000000009;

    [TestMethod]
    public async Task OnlyAnAdministratorSearchesThePlayers()
    {
        using var factory = new HubWebFactory();
        using var administrator = factory.Client(HubWebFactory.Administrator);
        using var player = factory.Client(HubWebFactory.Player);
        using var visitor = factory.Client();

        await factory.CompleteSetupAsync();

        var asAdministrator = await administrator.GetAsync(Api);
        var asPlayer = await player.GetAsync(Api);
        var asVisitor = await visitor.GetAsync(Api);

        Assert.AreEqual(HttpStatusCode.OK, asAdministrator.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, asPlayer.StatusCode);
        Assert.IsFalse(asVisitor.IsSuccessStatusCode);
    }

    [TestMethod]
    public async Task ThePlayersWhoPassedThroughTheServersComeMostRecentFirst()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await SeedAsync(factory);

        var result = await client.GetFromJsonAsync<JsonElement>(Api);
        var players = result.GetProperty("players").EnumerateArray().ToList();
        var alice = players.Single(player => player.GetProperty("communityId").GetString() == Alice.ToString());

        CollectionAssert.AreEqual(new[] { "CarolConnected", "BobPlayedYesterday", "AlicePlayed" }, players.Select(player => player.GetProperty("name").GetString()).ToArray());
        Assert.AreEqual(3, result.GetProperty("total").GetInt32());
        Assert.IsFalse(result.GetProperty("hasMore").GetBoolean());
        Assert.AreEqual("STEAM_1:1:19867136", alice.GetProperty("steamId").GetString());
        Assert.AreEqual("/avatar/76561198000000001", alice.GetProperty("avatarUrl").GetString());
        Assert.IsTrue(alice.GetProperty("knownOnSite").GetBoolean());
        Assert.IsFalse(string.IsNullOrEmpty(alice.GetProperty("lastSeen").GetString()));
        CollectionAssert.AreEqual(new[] { "AliceSmurf" }, alice.GetProperty("previousNames").EnumerateArray().Select(name => name.GetString()).ToArray());
    }

    [TestMethod]
    [DataRow("bob", "BobPlayedYesterday")]
    [DataRow("BOB", "BobPlayedYesterday")]
    [DataRow("smurf", "AlicePlayed")]
    [DataRow("STEAM_1:1:19867136", "AlicePlayed")]
    [DataRow("[U:1:39734275]", "CarolConnected")]
    [DataRow("https://steamcommunity.com/profiles/76561198000000002", "BobPlayedYesterday")]
    [DataRow("76561198000000003", "CarolConnected")]
    public async Task TheSearchFiltersByNameOrAnySteamId(string term, string expected)
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await SeedAsync(factory);

        var result = await client.GetFromJsonAsync<JsonElement>($"{Api}?q={Uri.EscapeDataString(term)}");
        var names = result.GetProperty("players").EnumerateArray().Select(player => player.GetProperty("name").GetString()).ToArray();

        CollectionAssert.AreEqual(new[] { expected }, names);
    }

    [TestMethod]
    public async Task ASteamIdThatNeverPlayedCanStillBeChosen()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await SeedAsync(factory);

        var result = await client.GetFromJsonAsync<JsonElement>($"{Api}?q={Uri.EscapeDataString("STEAM_0:1:19867140")}");
        var nobody = await client.GetFromJsonAsync<JsonElement>($"{Api}?q=nobody");
        var stranger = result.GetProperty("players").EnumerateArray().Single();

        Assert.AreEqual(Stranger.ToString(), stranger.GetProperty("communityId").GetString());
        Assert.IsTrue(Missing(stranger, "name"));
        Assert.IsTrue(Missing(stranger, "lastSeen"));
        Assert.IsFalse(stranger.GetProperty("knownOnSite").GetBoolean());
        Assert.AreEqual(0, nobody.GetProperty("players").GetArrayLength());
    }

    [TestMethod]
    public async Task TheSearchIsPaged()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();
            var now = DateTime.UtcNow;

            for (var index = 1; index <= 30; index++)
                dbContext.PlayerProfiles.Add(new PlayerProfile { CommunityId = Alice + index, Name = $"Player{index:00}", FirstSeenAtUtc = now.AddDays(-index), LastSeenAtUtc = now.AddDays(-index) });

            await dbContext.SaveChangesAsync();
        });

        var first = await client.GetFromJsonAsync<JsonElement>(Api);
        var second = await client.GetFromJsonAsync<JsonElement>($"{Api}?page=2");
        var beyond = await client.GetFromJsonAsync<JsonElement>($"{Api}?page=99");

        Assert.AreEqual(25, first.GetProperty("players").GetArrayLength());
        Assert.AreEqual("Player01", first.GetProperty("players")[0].GetProperty("name").GetString());
        Assert.AreEqual(30, first.GetProperty("total").GetInt32());
        Assert.IsTrue(first.GetProperty("hasMore").GetBoolean());
        Assert.AreEqual(5, second.GetProperty("players").GetArrayLength());
        Assert.AreEqual("Player26", second.GetProperty("players")[0].GetProperty("name").GetString());
        Assert.IsFalse(second.GetProperty("hasMore").GetBoolean());
        Assert.AreEqual(2, beyond.GetProperty("page").GetInt32());
    }

    [TestMethod]
    public async Task AFullLastPageIsNotFollowedByAnEmptyOne()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();
            var now = DateTime.UtcNow;

            for (var index = 1; index <= 25; index++)
                dbContext.PlayerProfiles.Add(new PlayerProfile { CommunityId = Alice + index, Name = $"Player{index:00}", FirstSeenAtUtc = now.AddDays(-index), LastSeenAtUtc = now.AddDays(-index) });

            await dbContext.SaveChangesAsync();
        });

        var first = await client.GetFromJsonAsync<JsonElement>(Api);
        var second = await client.GetFromJsonAsync<JsonElement>($"{Api}?page=2");

        Assert.AreEqual(25, first.GetProperty("players").GetArrayLength());
        Assert.IsFalse(first.GetProperty("hasMore").GetBoolean());
        Assert.AreEqual(1, second.GetProperty("page").GetInt32());
        Assert.AreEqual(25, second.GetProperty("players").GetArrayLength());
    }

    [TestMethod]
    [DataRow("/admin/administrators", "data-player-picker=\"#communityId\"")]
    [DataRow("/admin/player-connections", "data-player-picker=\"#pc-player-id\"")]
    public async Task ThePlayerFieldsOpenTheSharedSearchWindow(string page, string button)
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();

        var html = await client.GetStringAsync(page);

        Assert.Contains(button, html);
        Assert.Contains("id=\"pp-modal\"", html);
        Assert.Contains("\"playerSearchUrl\": \"/api/admin/players/search\"", html);
        Assert.AreEqual(1, html.Split("id=\"pp-modal\"").Length - 1);
    }

    [TestMethod]
    public async Task AnAdministratorCanBeAddedWithAnySteamIdFormat()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();

        var fields = new Dictionary<string, string> { ["communityId"] = "STEAM_1:1:19867136", ["name"] = "AliceTheAdmin" };
        var response = await HubWebFactory.PostFormAsync(client, "/admin/administrators", "/admin/administrators", fields);
        var added = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Administrators.AnyAsync(administrator => administrator.CommunityId == Alice));

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.IsTrue(added);
    }

    private static bool Missing(JsonElement element, string property)
    {
        return !element.TryGetProperty(property, out var value) || value.ValueKind == JsonValueKind.Null;
    }

    private static Task SeedAsync(HubWebFactory factory)
    {
        return factory.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();
            var now = DateTime.UtcNow;

            dbContext.PlayerProfiles.Add(new PlayerProfile { CommunityId = Alice, Name = "AlicePlayed", FirstSeenAtUtc = now.AddDays(-30), LastSeenAtUtc = now.AddDays(-3) });
            dbContext.PlayerProfiles.Add(new PlayerProfile { CommunityId = Bob, Name = "BobPlayedYesterday", FirstSeenAtUtc = now.AddDays(-30), LastSeenAtUtc = now.AddDays(-1) });
            dbContext.Set<PlayerConnection>().Add(new PlayerConnection { IpAddress = "200.100.50.1", CommunityId = Alice, Name = "AliceSmurf", FirstSeenAtUtc = now.AddDays(-20), LastSeenAtUtc = now.AddDays(-10), Count = 1 });
            dbContext.Set<PlayerConnection>().Add(new PlayerConnection { IpAddress = "200.100.50.2", CommunityId = Carol, Name = "CarolConnected", FirstSeenAtUtc = now.AddHours(-2), LastSeenAtUtc = now.AddHours(-1), Count = 1 });

            await dbContext.SaveChangesAsync();
            await services.GetRequiredService<IPlayerRegistryService>().BackfillAsync();
        });
    }
}
