using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Servers.Enums;
using L4D2PlayStats.Hub.Core.Modules.Servers.Models;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Core;

[TestClass]
public class GameServerTests
{
    [TestMethod]
    public async Task ANewServerGetsAnIdentifierFromItsName()
    {
        using var hub = new TestHub();

        var first = await hub.CreateServerAsync("L4D2 Zone | Competitive #1");
        var second = await hub.CreateServerAsync("L4D2 Zone | Competitive #1");

        Assert.AreEqual("l4d2-zone-competitive-1", first.Server.PublicId);
        Assert.AreEqual("l4d2-zone-competitive-1-2", second.Server.PublicId);
        Assert.AreEqual($"{first.Server.PublicId}:{first.Key}", first.Token);
    }

    [TestMethod]
    public async Task TheKeyIsStoredHashed()
    {
        using var hub = new TestHub();

        var credentials = await hub.CreateServerAsync();

        Assert.AreNotEqual(credentials.Key, credentials.Server.KeyHash);
        Assert.DoesNotContain(credentials.Key, credentials.Server.KeyHash);
    }

    [TestMethod]
    public async Task TheTokenAuthenticatesTheServer()
    {
        using var hub = new TestHub();

        var credentials = await hub.CreateServerAsync();
        var authentication = await AuthenticateAsync(hub, credentials.Token);

        Assert.IsTrue(authentication.Succeeded);
        Assert.AreEqual(credentials.Server.Id, authentication.Server!.Id);
    }

    [TestMethod]
    [DataRow(null, GameServerAuthenticationStatus.MissingToken)]
    [DataRow("", GameServerAuthenticationStatus.MissingToken)]
    [DataRow("no-separator", GameServerAuthenticationStatus.MissingToken)]
    [DataRow("unknown:key", GameServerAuthenticationStatus.UnknownServer)]
    [DataRow("test-server:wrong", GameServerAuthenticationStatus.InvalidKey)]
    public async Task ABadTokenIsRefused(string? token, GameServerAuthenticationStatus expected)
    {
        using var hub = new TestHub();

        await hub.CreateServerAsync();

        var authentication = await AuthenticateAsync(hub, token);

        Assert.IsFalse(authentication.Succeeded);
        Assert.AreEqual(expected, authentication.Status);
    }

    [TestMethod]
    public async Task ADisabledServerIsRefused()
    {
        using var hub = new TestHub();

        var credentials = await hub.CreateServerAsync();

        await hub.RunAsync(services => services.GetRequiredService<IGameServerService>().UpdateAsync(credentials.Server.Id, "Test Server", null, false));

        var authentication = await AuthenticateAsync(hub, credentials.Token);

        Assert.AreEqual(GameServerAuthenticationStatus.Disabled, authentication.Status);
        Assert.IsFalse(authentication.Succeeded);
    }

    [TestMethod]
    public async Task RegeneratingTheKeyInvalidatesTheOldOne()
    {
        using var hub = new TestHub();

        var credentials = await hub.CreateServerAsync();
        var regenerated = await hub.RunAsync(services => services.GetRequiredService<IGameServerService>().RegenerateKeyAsync(credentials.Server.Id));

        Assert.IsFalse((await AuthenticateAsync(hub, credentials.Token)).Succeeded);
        Assert.IsTrue((await AuthenticateAsync(hub, regenerated!.Token)).Succeeded);
    }

    [TestMethod]
    public async Task TheStatusFollowsTheSignals()
    {
        using var hub = new TestHub();

        var credentials = await hub.CreateServerAsync();

        Assert.AreEqual(GameServerConnectionState.Waiting, Status(hub, credentials.Server).State);

        await AuthenticateAsync(hub, $"{credentials.Server.PublicId}:wrong");
        Assert.AreEqual(GameServerConnectionState.InvalidKey, Status(hub, credentials.Server).State);

        hub.Clock.Advance(TimeSpan.FromSeconds(5));
        await SignalAsync(hub, credentials);
        Assert.AreEqual(GameServerConnectionState.Online, Status(hub, credentials.Server).State);

        hub.Clock.Advance(TimeSpan.FromMinutes(5));
        Assert.AreEqual(GameServerConnectionState.Offline, Status(hub, credentials.Server).State);
    }

    [TestMethod]
    public async Task TheLastContactSurvivesARestart()
    {
        using var hub = new TestHub();

        var credentials = await hub.CreateServerAsync();

        await SignalAsync(hub, credentials);

        var server = await hub.RunAsync(services => services.GetRequiredService<IGameServerService>().FindAsync(credentials.Server.Id));

        Assert.AreEqual(hub.Clock.UtcNow, server!.LastSeenAtUtc);
    }

    [TestMethod]
    public async Task AServerWithoutStatisticsCanBeDeleted()
    {
        using var hub = new TestHub();

        var credentials = await hub.CreateServerAsync();
        var deleted = await hub.RunAsync(services => services.GetRequiredService<IGameServerService>().DeleteAsync(credentials.Server.Id));

        Assert.IsTrue(deleted);
    }

    [TestMethod]
    public async Task AServerWithStatisticsCannotBeDeleted()
    {
        using var hub = new TestHub();

        var credentials = await hub.CreateServerAsync();

        await hub.UploadAsync(credentials.Server, StatisticsBuilder.Sample());

        var deleted = await hub.RunAsync(services => services.GetRequiredService<IGameServerService>().DeleteAsync(credentials.Server.Id));

        Assert.IsFalse(deleted);
    }

    private static Task<GameServerAuthentication> AuthenticateAsync(TestHub hub, string? token)
    {
        return hub.RunAsync(services => services.GetRequiredService<IGameServerService>().AuthenticateAsync(token));
    }

    private static Task SignalAsync(TestHub hub, GameServerCredentials credentials)
    {
        return hub.RunAsync(async services =>
        {
            var service = services.GetRequiredService<IGameServerService>();
            var server = await service.FindAsync(credentials.Server.Id);

            await service.SignalAsync(server!, new GameServerSignal { Hostname = "test" });
        });
    }

    private static GameServerStatus Status(TestHub hub, GameServer server)
    {
        using var scope = hub.Scope();

        return scope.ServiceProvider.GetRequiredService<IGameServerPresence>().GetStatus(server);
    }
}
