using System.Net;
using System.Text.Json;
using L4D2PlayStats.Hub.Core.Modules.Live.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;
using L4D2PlayStats.Hub.Core.Modules.Live.Services;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web.Live;

[TestClass]
public class LivePluginApiTests
{
    private const string Players = """
                                   {
                                     "survivors": [
                                       {"communityId":"76561198000000001","name":"Alice","isAdmin":false,"latency":0.05,"character":0,"permanentHealth":80,"temporaryHealth":10,"primaryWeapon":8,"secondaryWeapon":19,"meleeWeapon":11,"slotNumber3":13,"slotNumber4":0,"slotNumber5":15,"blackAndWhite":false,"incapacitated":false,"isPlayerAlive":true,"progress":0.42}
                                     ],
                                     "infecteds": [
                                       {"communityId":"76561198000000002","name":"Bob","isAdmin":true,"latency":0.15,"type":3,"health":250,"maxHealth":250,"damage":37,"isInfectedGhost":false,"isPlayerAlive":true}
                                     ],
                                     "spectators": [
                                       {"communityId":"76561198000000003","name":"Carol","isAdmin":false}
                                     ]
                                   }
                                   """;

    private static readonly (HttpMethod Method, string Path)[] PluginEndpoints =
    [
        (HttpMethod.Put, "/api/game-info/configuration"),
        (HttpMethod.Put, "/api/game-info/round"),
        (HttpMethod.Put, "/api/game-info/scoreboard"),
        (HttpMethod.Put, "/api/game-info/players"),
        (HttpMethod.Put, "/api/game-info/messages"),
        (HttpMethod.Post, "/api/game-info/events"),
        (HttpMethod.Post, "/api/game-info/player-connection-info"),
        (HttpMethod.Get, "/api/game-info/server-command/dequeue"),
        (HttpMethod.Get, "/api/external-chat")
    ];

    [TestMethod]
    [DataRow(null)]
    [DataRow("unknown:key")]
    [DataRow("test-server:wrong")]
    [DataRow("l4d2-web-shared-secret")]
    public async Task EveryPluginEndpointRefusesAMissingOrWrongToken(string? token)
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        await factory.CreateServerAsync();
        await factory.EnableAsync();

        if (token != null)
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);

        foreach (var endpoint in PluginEndpoints)
        {
            var response = await SendAsync(client, endpoint.Method, endpoint.Path);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, endpoint.Path);
        }
    }

    [TestMethod]
    public async Task EveryPluginEndpointAcceptsTheTokenOfTheServer()
    {
        using var factory = new LiveWebFactory();

        var credentials = await factory.CreateServerAsync();
        using var client = factory.ServerClient(credentials);

        await factory.EnableAsync();

        foreach (var endpoint in PluginEndpoints.Where(endpoint => !endpoint.Path.EndsWith("player-connection-info", StringComparison.Ordinal)))
        {
            var response = await SendAsync(client, endpoint.Method, endpoint.Path);

            Assert.IsTrue(response.IsSuccessStatusCode, endpoint.Path);
        }
    }

    [TestMethod]
    public async Task ADisabledServerCannotSendLiveData()
    {
        using var factory = new LiveWebFactory();

        var credentials = await factory.CreateServerAsync();
        using var client = factory.ServerClient(credentials);

        await factory.RunAsync(services => services.GetRequiredService<IGameServerService>().UpdateAsync(credentials.Server.Id, "Test Server", null, false));

        var response = await SendAsync(client, HttpMethod.Put, "/api/game-info/round");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task WhileTheLiveServerModuleIsOffEveryLiveRouteIsNotFound()
    {
        using var factory = new LiveWebFactory();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);
        using var visitor = factory.Client();

        await factory.CompleteSetupAsync();
        await factory.EnableAsync(false);

        foreach (var endpoint in PluginEndpoints)
        {
            var response = await SendAsync(server, endpoint.Method, endpoint.Path);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, endpoint.Path);
        }

        foreach (var path in new[] { "/servers", "/servers/test-server", "/servers/test-server/header", "/servers/test-server/players", "/servers/test-server/feed", "/api/servers", "/api/servers/test-server/game-info" })
        {
            var response = await visitor.GetAsync(path);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, path);
        }
    }

    [TestMethod]
    public async Task WithChatAndCommandsOffThePluginGetsEmptyAnswersInsteadOfErrors()
    {
        using var factory = new LiveWebFactory();

        var credentials = await factory.CreateServerAsync();
        using var client = factory.ServerClient(credentials);

        await factory.EnableAsync(true, false, false);

        var round = await SendAsync(client, HttpMethod.Put, "/api/game-info/round");
        var chat = await client.GetAsync("/api/external-chat");
        var command = await client.GetAsync("/api/game-info/server-command/dequeue");

        Assert.AreEqual(HttpStatusCode.OK, round.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, chat.StatusCode);
        Assert.AreEqual("[]", await chat.Content.ReadAsStringAsync());
        Assert.AreEqual(HttpStatusCode.NoContent, command.StatusCode);
    }

    [TestMethod]
    public async Task TheLiveMatchSentByThePluginIsPublishedAfterTheDelay()
    {
        using var factory = new LiveWebFactory();

        var credentials = await factory.CreateServerAsync();
        using var client = factory.ServerClient(credentials);
        using var visitor = factory.Client();

        await client.PutAsync("/api/game-info/configuration", LiveWebFactory.Json("""{"teamSize":4,"name":"ZoneMod v2.9.2b"}"""));
        await client.PutAsync("/api/game-info/round", LiveWebFactory.Json("""{"isInReady":false,"isInPause":false,"inSecondHalfOfRound":true,"maxChapterProgressPoints":400,"tankPercent":0.62,"witchPercent":0,"antiCheatStatus":1}"""));
        await client.PutAsync("/api/game-info/scoreboard", LiveWebFactory.Json("""{"survivorScore":812,"infectedScore":640,"bonus":120,"maxBonus":400,"currentProgress":0.5,"currentProgressPoints":200,"isTankInPlay":true,"tankIsDead":false}"""));
        await client.PutAsync("/api/game-info/players", LiveWebFactory.Json(Players));

        var before = await visitor.GetAsync("/api/servers/test-server/game-info/scoreboard");

        factory.PassTheDelay();

        var after = await visitor.GetFromJsonAsync<JsonElement>("/api/servers/test-server/game-info");
        var snapshot = factory.Services.GetRequiredService<ILiveServerStore>().For(credentials.Server.Id).Snapshot();
        var status = factory.Services.GetRequiredService<IGameServerPresence>().GetStatus(credentials.Server);

        Assert.AreEqual(HttpStatusCode.NoContent, before.StatusCode);
        Assert.AreEqual("test-server", after.GetProperty("server").GetProperty("publicId").GetString());
        Assert.AreEqual("ZoneMod v2.9.2b", after.GetProperty("configuration").GetProperty("name").GetString());
        Assert.AreEqual(812, after.GetProperty("scoreboard").GetProperty("survivorScore").GetInt32());
        Assert.IsTrue(after.GetProperty("round").GetProperty("inSecondHalfOfRound").GetBoolean());
        Assert.AreEqual("76561198000000001", after.GetProperty("survivors")[0].GetProperty("communityId").GetString());
        Assert.IsTrue(after.GetProperty("anyPlayerConnected").GetBoolean());
        Assert.AreEqual(AntiCheatStatus.Enabled, snapshot.Round!.AntiCheatStatus);
        Assert.IsTrue(snapshot.Round.WitchDisabled);
        Assert.AreEqual(Weapon.ShotgunChrome, snapshot.Survivors[0].PrimaryWeapon);
        Assert.AreEqual(MeleeWeapon.Katana, snapshot.Survivors[0].MeleeWeapon);
        Assert.AreEqual(90, snapshot.Survivors[0].Health);
        Assert.AreEqual(LatencyType.Low, snapshot.Survivors[0].LatencyType);
        Assert.AreEqual(InfectedType.Hunter, snapshot.Infecteds[0].Type);
        Assert.AreEqual(37, snapshot.Infecteds[0].Damage);
        Assert.AreEqual("Carol", snapshot.Spectators[0].Name);
        Assert.IsNotNull(status.LastLiveDataAtUtc);
    }

    [TestMethod]
    public async Task EventsAndChatMessagesOfThePluginReachTheFeed()
    {
        using var factory = new LiveWebFactory();

        var credentials = await factory.CreateServerAsync();
        using var client = factory.ServerClient(credentials);
        using var visitor = factory.Client();

        var skeet = await client.PostAsync("/api/game-info/events", LiveWebFactory.Json("""{"type":"skeet","actor":{"name":"Alice","isBot":false,"communityId":"76561198000000001"},"hunter":{"name":"Hunter","isBot":true},"skeetType":"Sniper","isTeamSkeet":false}"""));
        var alarm = await client.PostAsync("/api/game-info/events", LiveWebFactory.Json("""{"type":"carAlarmTriggered","actor":{"name":"Alice","isBot":false,"communityId":"76561198000000001"},"reason":2}"""));
        var unknown = await client.PostAsync("/api/game-info/events", LiveWebFactory.Json("""{"type":"somethingNew","actor":{"name":"Alice","isBot":false}}"""));
        var message = await client.PutAsync("/api/game-info/messages", LiveWebFactory.Json("""{"public":false,"team":3,"communityId":"76561198000000002","name":"Bob","isAdmin":true,"message":"rush them"}"""));

        factory.PassTheDelay();

        var feed = factory.Services.GetRequiredService<ILiveServerStore>().For(credentials.Server.Id).Feed;
        var events = await visitor.GetFromJsonAsync<JsonElement>("/api/servers/test-server/game-info/events");
        var messages = await visitor.GetFromJsonAsync<JsonElement>("/api/servers/test-server/game-info/messages");

        Assert.AreEqual(HttpStatusCode.OK, skeet.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, alarm.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, unknown.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, message.StatusCode);
        Assert.HasCount(4, feed);
        Assert.AreEqual(SkeetType.Sniper, feed.OfType<SkeetEvent>().Single().SkeetType);
        Assert.AreEqual(1, feed.OfType<SkeetEvent>().Single().Stars);
        Assert.AreEqual(CarAlarmReason.Touched, feed.OfType<CarAlarmTriggeredEvent>().Single().Reason);
        Assert.AreEqual(GameEventType.Unknown, feed.OfType<GameEvent>().Last().EventType);
        Assert.AreEqual(LiveTeam.Infected, feed.OfType<LiveChatMessage>().Single().Team);
        Assert.AreEqual(3, events.GetArrayLength());
        Assert.AreEqual("skeet", events[0].GetProperty("type").GetString());
        Assert.AreEqual("Alice", events[0].GetProperty("actor").GetProperty("name").GetString());
        Assert.AreEqual(1, messages.GetArrayLength());
        Assert.AreEqual("rush them", messages[0].GetProperty("message").GetString());
        Assert.AreEqual("Bob", messages[0].GetProperty("player").GetProperty("name").GetString());
    }

    [TestMethod]
    public async Task TheBodiesOfThePluginAreAcceptedWithoutTheirOptionalFields()
    {
        using var factory = new LiveWebFactory();

        var credentials = await factory.CreateServerAsync();
        using var client = factory.ServerClient(credentials);

        var configuration = await client.PutAsync("/api/game-info/configuration", LiveWebFactory.Json("""{"teamSize":4}"""));
        var round = await client.PutAsync("/api/game-info/round", LiveWebFactory.Json("""{"isInReady":true,"inSecondHalfOfRound":false,"maxChapterProgressPoints":400,"tankPercent":0.5,"witchPercent":0.75}"""));
        var players = await client.PutAsync("/api/game-info/players", LiveWebFactory.Json("""{"survivors":[],"infecteds":[],"spectators":[]}"""));
        var roundLive = await client.PostAsync("/api/game-info/events", LiveWebFactory.Json("""{"type":"roundLive","secondHalf":true}"""));
        var mixStarted = await client.PostAsync("/api/game-info/events", LiveWebFactory.Json("""{"type":"mixStarted"}"""));

        factory.PassTheDelay();

        var snapshot = factory.Services.GetRequiredService<ILiveServerStore>().For(credentials.Server.Id).Snapshot();

        Assert.AreEqual(HttpStatusCode.OK, configuration.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, round.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, players.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, roundLive.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, mixStarted.StatusCode);
        Assert.AreEqual(4, snapshot.Configuration!.TeamSize);
        Assert.IsNull(snapshot.Configuration.Name);
        Assert.IsTrue(snapshot.Round!.IsInReady);
        Assert.IsFalse(snapshot.Round.IsInPause);
        Assert.IsNull(snapshot.Round.AntiCheatStatus);
        Assert.AreEqual(0.75m, snapshot.Round.WitchPercent);
        Assert.IsFalse(snapshot.AnyPlayerConnected);
        Assert.IsTrue(snapshot.Events.OfType<RoundLiveEvent>().Single().SecondHalf);
        Assert.IsNull(snapshot.Events.OfType<MixStartedEvent>().Single().Actor);
    }

    [TestMethod]
    public async Task TwoServersSendingAtTheSameTimeKeepSeparateMatches()
    {
        using var factory = new LiveWebFactory();

        var first = await factory.CreateServerAsync("First");
        var second = await factory.CreateServerAsync("Second");
        using var firstClient = factory.ServerClient(first);
        using var secondClient = factory.ServerClient(second);
        using var visitor = factory.Client();

        await firstClient.PutAsync("/api/game-info/scoreboard", LiveWebFactory.Json("""{"survivorScore":100,"infectedScore":0}"""));
        await secondClient.PutAsync("/api/game-info/scoreboard", LiveWebFactory.Json("""{"survivorScore":7,"infectedScore":9}"""));

        factory.PassTheDelay();

        var firstScore = await visitor.GetFromJsonAsync<JsonElement>("/api/servers/first/game-info/scoreboard");
        var secondScore = await visitor.GetFromJsonAsync<JsonElement>("/api/servers/second/game-info/scoreboard");
        var list = await visitor.GetFromJsonAsync<JsonElement>("/api/servers");

        Assert.AreEqual(100, firstScore.GetProperty("survivorScore").GetInt32());
        Assert.AreEqual(7, secondScore.GetProperty("survivorScore").GetInt32());
        Assert.AreEqual(2, list.GetArrayLength());
        Assert.AreEqual("Online", list[0].GetProperty("state").GetString());
    }

    [TestMethod]
    public async Task ThePluginReadsTheSiteChatOfItsOwnServer()
    {
        using var factory = new LiveWebFactory();

        var first = await factory.CreateServerAsync("First");
        var second = await factory.CreateServerAsync("Second");
        using var firstClient = factory.ServerClient(first);
        using var secondClient = factory.ServerClient(second);

        await factory.EnableAsync();
        await factory.RunAsync(services => services.GetRequiredService<ISiteChatService>().SendAsync(first.Server, new SiteChatUser(HubWebFactory.Player, "Alice"), "hello server"));

        var messages = await firstClient.GetFromJsonAsync<JsonElement>("/api/external-chat");
        var ticks = messages[0].GetProperty("ticks").GetString();
        var newer = await firstClient.GetFromJsonAsync<JsonElement>($"/api/external-chat?after={ticks}");
        var other = await secondClient.GetFromJsonAsync<JsonElement>("/api/external-chat");

        var response = await firstClient.GetAsync($"/api/external-chat?after={ticks}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("application/json", response.Content.Headers.ContentType!.MediaType);
        Assert.AreEqual("[]", await response.Content.ReadAsStringAsync());
        Assert.AreEqual(JsonValueKind.Array, messages.ValueKind);
        Assert.AreEqual(1, messages.GetArrayLength());
        Assert.AreEqual(JsonValueKind.String, messages[0].GetProperty("ticks").ValueKind);
        Assert.AreEqual("hello server", messages[0].GetProperty("text").GetString());
        Assert.AreEqual("Alice", messages[0].GetProperty("name").GetString());
        Assert.AreEqual(HubWebFactory.Player.ToString(), messages[0].GetProperty("communityId").GetString());
        Assert.AreEqual("STEAM_1:0:19867236", messages[0].GetProperty("steamId").GetString());
        Assert.AreEqual($"https://steamcommunity.com/profiles/{HubWebFactory.Player}", messages[0].GetProperty("profileUrl").GetString());
        Assert.AreEqual(factory.Clock.UtcNow.Ticks.ToString(), ticks);
        Assert.AreEqual(0, newer.GetArrayLength());
        Assert.AreEqual(0, other.GetArrayLength());
    }

    [TestMethod]
    public async Task ThePluginTakesTheNextCommandFromItsOwnQueue()
    {
        using var factory = new LiveWebFactory();

        var first = await factory.CreateServerAsync("First");
        var second = await factory.CreateServerAsync("Second");
        using var firstClient = factory.ServerClient(first);
        using var secondClient = factory.ServerClient(second);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();
        await factory.RunAsync(services => services.GetRequiredService<IRemoteCommandService>().QueueAsync(first.Server, HubWebFactory.Administrator, "Admin", "!kick Alice"));

        var other = await secondClient.GetAsync("/api/game-info/server-command/dequeue");
        var command = await firstClient.GetAsync("/api/game-info/server-command/dequeue");
        var body = await command.Content.ReadFromJsonAsync<JsonElement>();
        var empty = await firstClient.GetAsync("/api/game-info/server-command/dequeue");

        Assert.AreEqual(HttpStatusCode.NoContent, other.StatusCode);
        Assert.AreEqual(string.Empty, await other.Content.ReadAsStringAsync());
        Assert.AreEqual(HttpStatusCode.OK, command.StatusCode);
        Assert.AreEqual("application/json", command.Content.Headers.ContentType!.MediaType);
        Assert.AreEqual(JsonValueKind.Object, body.ValueKind);
        Assert.AreEqual("sm_kick Alice", body.GetProperty("fullCommand").GetString());
        Assert.AreEqual("kick", body.GetProperty("command").GetString());
        Assert.AreEqual("Alice", body.GetProperty("arguments").GetString());
        Assert.AreEqual(HttpStatusCode.NoContent, empty.StatusCode);
    }

    [TestMethod]
    public async Task AConnectionAnswersWithThePlayersOnTheSameAddress()
    {
        using var factory = new LiveWebFactory();

        var first = await factory.CreateServerAsync("First");
        var second = await factory.CreateServerAsync("Second");
        using var firstClient = factory.ServerClient(first);
        using var secondClient = factory.ServerClient(second);

        var alone = await firstClient.PostAsync("/api/game-info/player-connection-info", LiveWebFactory.Json("""{"communityId":"76561198000000001","ipAddress":"200.100.50.1","name":"Alice"}"""));
        var together = await secondClient.PostAsync("/api/game-info/player-connection-info", LiveWebFactory.Json("""{"communityId":"76561198000000002","ipAddress":"200.100.50.1","name":"Bob"}"""));
        var invalid = await firstClient.PostAsync("/api/game-info/player-connection-info", LiveWebFactory.Json("""{"communityId":"76561198000000001","ipAddress":"nowhere","name":"Alice"}"""));

        var none = await alone.Content.ReadFromJsonAsync<JsonElement>();
        var related = await together.Content.ReadFromJsonAsync<JsonElement>();
        var text = await together.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, alone.StatusCode);
        Assert.AreEqual("application/json", alone.Content.Headers.ContentType!.MediaType);
        Assert.AreEqual("application/json", together.Content.Headers.ContentType!.MediaType);
        Assert.AreEqual(JsonValueKind.Array, none.ValueKind);
        Assert.AreEqual(0, none.GetArrayLength());
        Assert.AreEqual(1, related.GetArrayLength());
        Assert.AreEqual("Alice", related[0].GetProperty("name").GetString());
        Assert.AreEqual("76561198000000001", related[0].GetProperty("communityId").GetString());
        Assert.AreEqual("STEAM_1:1:19867136", related[0].GetProperty("steamId").GetString());
        Assert.AreEqual("https://steamcommunity.com/profiles/76561198000000001", related[0].GetProperty("profileUrl").GetString());
        Assert.AreEqual(1, related[0].GetProperty("connectionCount").GetInt64());
        Assert.DoesNotContain("200.100.50.1", text);
        Assert.AreEqual(HttpStatusCode.BadRequest, invalid.StatusCode);
    }

    private static Task<HttpResponseMessage> SendAsync(HttpClient client, HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, path);

        if (method != HttpMethod.Get)
            request.Content = LiveWebFactory.Json("{}");

        return client.SendAsync(request);
    }
}
