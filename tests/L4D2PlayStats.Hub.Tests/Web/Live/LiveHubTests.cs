using System.Net;
using System.Threading.Channels;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Live;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;
using WebConsts = L4D2PlayStats.Hub.Web.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Web.Live;

[TestClass]
public class LiveHubTests
{
    private const long AliceId = 76561198000000001;
    private const string Alice = "76561198000000001";

    private static readonly TimeSpan Wait = TimeSpan.FromSeconds(10);

    [TestMethod]
    public async Task APageWatchesOnlyAnEnabledServer()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        await factory.CreateServerAsync();
        var disabled = await factory.CreateServerAsync("Disabled");

        await factory.RunAsync(services => services.GetRequiredService<IGameServerService>().UpdateAsync(disabled.Server.Id, "Disabled", null, false));

        await using var connection = await ConnectAsync(factory);

        Assert.IsFalse(await connection.InvokeAsync<bool>(nameof(LiveHub.Watch), "nothing-here"));
        Assert.IsFalse(await connection.InvokeAsync<bool>(nameof(LiveHub.Watch), "disabled"));
        Assert.IsFalse(await connection.InvokeAsync<bool>(nameof(LiveHub.Watch), ""));
        Assert.AreEqual(0, Watchers(factory).Count);
        Assert.IsTrue(await connection.InvokeAsync<bool>(nameof(LiveHub.Watch), "test-server"));
        Assert.AreEqual(1, Watchers(factory).Count);
    }

    [TestMethod]
    public async Task NoServerIsWatchedWithTheLiveServerModuleOff()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();
        await factory.CreateServerAsync();
        await factory.EnableAsync(false, false, false);

        await using var connection = await ConnectAsync(factory);

        Assert.IsFalse(await connection.InvokeAsync<bool>(nameof(LiveHub.Watch), "test-server"));
        Assert.AreEqual(0, Watchers(factory).Count);
    }

    [TestMethod]
    public async Task ThePageIsNotifiedOnlyAfterTheDelay()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();

        await using var connection = await ConnectAsync(factory);
        var changes = Listen(connection);

        Assert.IsTrue(await connection.InvokeAsync<bool>(nameof(LiveHub.Watch), "test-server"));

        await NotifyAsync(factory);

        await factory.RunAsync(services =>
        {
            var live = services.GetRequiredService<ILiveDataService>();

            live.UpdatePlayers(credentials.Server, new PlayersCommand { Survivors = [new LiveSurvivor { CommunityId = Alice, Name = "Alice", PermanentHealth = 80 }] });
            live.AddMessage(credentials.Server, new ChatMessageCommand { CommunityId = Alice, Name = "Alice", Message = "tank incoming" });

            return Task.CompletedTask;
        });

        await NotifyAsync(factory);

        factory.Clock.Advance(TimeSpan.FromSeconds(CoreConsts.LiveDelayInSeconds - 1));

        await NotifyAsync(factory);

        factory.Clock.Advance(TimeSpan.FromSeconds(1));

        await NotifyAsync(factory);

        var first = await ReadAsync(changes);

        await NotifyAsync(factory);
        await Task.Delay(TimeSpan.FromMilliseconds(300));

        CollectionAssert.AreEqual(new[] { WebConsts.LiveChangeHeader, WebConsts.LiveChangePlayers, WebConsts.LiveChangeFeed }, first);
        Assert.IsFalse(changes.TryRead(out _));
    }

    [TestMethod]
    public async Task AChangeRightAfterThePageStartsWatchingIsNotLost()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();

        await using var connection = await ConnectAsync(factory);
        var changes = Listen(connection);

        Assert.IsTrue(await connection.InvokeAsync<bool>(nameof(LiveHub.Watch), "test-server"));

        await factory.RunAsync(services =>
        {
            services.GetRequiredService<ILiveDataService>().AddMessage(credentials.Server, new ChatMessageCommand { CommunityId = Alice, Name = "Alice", Message = "right away" });

            return Task.CompletedTask;
        });

        factory.Clock.Advance(TimeSpan.FromSeconds(CoreConsts.LiveDelayInSeconds));

        await NotifyAsync(factory);

        CollectionAssert.Contains(await ReadAsync(changes), WebConsts.LiveChangeFeed);
    }

    [TestMethod]
    public async Task OnlyThePagesOfTheChangedServerAreNotified()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var first = await factory.CreateServerAsync("First");
        await factory.CreateServerAsync("Second");
        await factory.EnableAsync();

        await using var firstConnection = await ConnectAsync(factory);
        await using var secondConnection = await ConnectAsync(factory);
        var firstChanges = Listen(firstConnection);
        var secondChanges = Listen(secondConnection);

        Assert.IsTrue(await firstConnection.InvokeAsync<bool>(nameof(LiveHub.Watch), "first"));
        Assert.IsTrue(await secondConnection.InvokeAsync<bool>(nameof(LiveHub.Watch), "second"));

        await NotifyAsync(factory);

        await factory.RunAsync(services => services.GetRequiredService<ISiteChatService>().SendAsync(first.Server, new SiteChatUser(AliceId, "Alice"), "hello from the site"));

        await NotifyAsync(factory);

        var changed = await ReadAsync(firstChanges);

        await Task.Delay(TimeSpan.FromMilliseconds(300));

        CollectionAssert.AreEqual(new[] { WebConsts.LiveChangeFeed }, changed);
        Assert.IsFalse(secondChanges.TryRead(out _));
    }

    [TestMethod]
    public async Task AClosedPageStopsBeingWatched()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();
        await factory.CreateServerAsync();

        var connection = await ConnectAsync(factory);

        Assert.IsTrue(await connection.InvokeAsync<bool>(nameof(LiveHub.Watch), "test-server"));

        await connection.DisposeAsync();

        var watchers = Watchers(factory);
        var timeout = DateTime.UtcNow + Wait;

        while (watchers.Count > 0 && DateTime.UtcNow < timeout)
            await Task.Delay(50);

        Assert.AreEqual(0, watchers.Count);
    }

    [TestMethod]
    public async Task TheHubAsksProxiesNotToBufferItsResponses()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var response = await client.PostAsync($"{WebConsts.LiveHubUrl}/negotiate?negotiateVersion=1", null);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(WebConsts.ProxyBufferingDisabled, response.Headers.GetValues(WebConsts.ProxyBufferingHeader).Single());
    }

    [TestMethod]
    public async Task TheLivePageListensForChangesAndKeepsPollingAsAFallback()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();
        await factory.CreateServerAsync();

        var page = await client.GetStringAsync("/servers/test-server");

        Assert.Contains("/lib/signalr/signalr.min.js", page);
        Assert.Contains($"var liveHubUrl = '{WebConsts.LiveHubUrl}';", page);
        Assert.Contains("var serverPublicId = \"test-server\";", page);
        Assert.Contains($"var refreshInterval = {CoreConsts.LiveRefreshInMilliseconds};", page);
        Assert.Contains($"var safetyRefreshInterval = {WebConsts.LivePageSafetyRefreshInMilliseconds};", page);
        Assert.Contains("if (typeof signalR === 'undefined' || !window.Promise)", page);
        Assert.Contains($"connection.on('{WebConsts.LiveHubChangedMethod}'", page);
        Assert.Contains($"connection.invoke('{nameof(LiveHub.Watch)}', serverPublicId)", page);
        Assert.Contains("pushConnected ? safetyRefreshInterval : interval", page);
        Assert.Contains("url: serverUrl + '/header'", page);
        Assert.Contains("url: serverUrl + '/players'", page);
        Assert.Contains("url: serverUrl + '/feed?after=' + after", page);
    }

    private static async Task<HubConnection> ConnectAsync(LiveWebFactory factory)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(factory.Server.BaseAddress, WebConsts.LiveHubUrl), options =>
            {
                options.Transports = HttpTransportType.LongPolling;
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();

        return connection;
    }

    private static ChannelReader<string[]> Listen(HubConnection connection)
    {
        var channel = Channel.CreateUnbounded<string[]>();

        connection.On<string[]>(WebConsts.LiveHubChangedMethod, changes => channel.Writer.TryWrite(changes));

        return channel.Reader;
    }

    private static async Task<string[]> ReadAsync(ChannelReader<string[]> changes)
    {
        using var cancellation = new CancellationTokenSource(Wait);

        return await changes.ReadAsync(cancellation.Token);
    }

    private static Task NotifyAsync(LiveWebFactory factory)
    {
        return factory.Services.GetRequiredService<LiveNotifier>().NotifyAsync();
    }

    private static LiveWatchers Watchers(LiveWebFactory factory)
    {
        return factory.Services.GetRequiredService<LiveWatchers>();
    }
}