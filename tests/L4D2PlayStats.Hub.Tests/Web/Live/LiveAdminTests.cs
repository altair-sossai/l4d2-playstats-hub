using System.Net;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web.Live;

[TestClass]
public class LiveAdminTests
{
    private const long Alice = 76561198000000001;

    [TestMethod]
    public async Task TheAdministratorBansAndUnbansAPlayerFromTheChat()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();

        var add = await LiveWebFactory.SendJsonAsync(client, "/admin/chat-bans", HttpMethod.Post, "/api/chat-bans", $$"""{"communityId":"{{Alice}}","name":"AliceTheSpammer","reason":"Flooding the chat"}""");
        var listed = await client.GetStringAsync("/admin/chat-bans");
        var ban = await factory.RunAsync(services => services.GetRequiredService<IChatBanService>().FindAsync(Alice));

        var remove = await LiveWebFactory.SendJsonAsync(client, "/admin/chat-bans", HttpMethod.Delete, $"/api/chat-bans/{Alice}", null);
        var after = await factory.RunAsync(services => services.GetRequiredService<IChatBanService>().ListAsync());

        Assert.AreEqual(HttpStatusCode.OK, add.StatusCode);
        Assert.Contains("AliceTheSpammer", listed);
        Assert.Contains("Flooding the chat", listed);
        Assert.AreEqual(HubWebFactory.Administrator, ban!.CreatedByCommunityId);
        Assert.AreEqual(HttpStatusCode.NoContent, remove.StatusCode);
        Assert.IsEmpty(after);
    }

    [TestMethod]
    public async Task TheChatBansPageListsWhoWroteRecently()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();

        var credentials = await factory.CreateServerAsync();

        await factory.RunAsync(services => services.GetRequiredService<ISiteChatService>().SendAsync(credentials.Server, new SiteChatUser(Alice, "AliceTheChatter"), "buy my skins"));

        var page = await client.GetStringAsync("/admin/chat-bans");
        var senders = await client.GetStringAsync("/api/chat-bans/recent-senders");

        Assert.Contains("cb-recent", page);
        Assert.Contains("AliceTheChatter", senders);
        Assert.Contains("buy my skins", senders);
    }

    [TestMethod]
    public async Task TheAdministratorQueuesACommandAndSeesItInTheAuditLog()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        var fields = new Dictionary<string, string> { ["serverId"] = credentials.Server.Id.ToString(), ["command"] = "!forcestart" };
        var queue = await HubWebFactory.PostFormAsync(client, "/admin/remote-commands", "/admin/remote-commands", fields);
        var command = await server.GetStringAsync("/api/game-info/server-command/dequeue");
        var page = await client.GetStringAsync("/admin/remote-commands");
        var logs = await factory.RunAsync(services => services.GetRequiredService<IRemoteCommandService>().ListAsync(0, 10));

        Assert.AreEqual(HttpStatusCode.Redirect, queue.StatusCode);
        Assert.Contains("\"fullCommand\":\"sm_forcestart\"", command);
        Assert.Contains("sm_forcestart", page);
        Assert.Contains("Test Server", page);
        Assert.HasCount(1, logs);
        Assert.AreEqual(HubWebFactory.Administrator, logs[0].AdministratorCommunityId);
        Assert.IsNotNull(logs[0].DeliveredAtUtc);
    }

    [TestMethod]
    public async Task TheRemoteCommandsPageAndTheLivePageShareTheCommandHelper()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();

        var withoutServers = await client.GetStringAsync("/admin/remote-commands");

        await factory.CreateServerAsync();

        var page = await client.GetStringAsync("/admin/remote-commands");
        var live = await client.GetStringAsync("/servers");

        Assert.DoesNotContain("command-builder.js", withoutServers);
        Assert.Contains("onclick=\"terminal.show()\"", page);
        Assert.Contains("command-builder.js", page);
        Assert.Contains("'!forcematch'", page);
        Assert.Contains("id=\"command\" name=\"command\"", page);
        Assert.Contains("command-builder.js", live);
        Assert.Contains("'!forcematch'", live);
    }

    [TestMethod]
    public async Task AnInvalidCommandIsRefusedWithAnExplanation()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        var fields = new Dictionary<string, string> { ["serverId"] = credentials.Server.Id.ToString(), ["command"] = "quit" };

        await HubWebFactory.PostFormAsync(client, "/admin/remote-commands", "/admin/remote-commands", fields);

        var page = await client.GetStringAsync("/admin/remote-commands");
        var command = await server.GetAsync("/api/game-info/server-command/dequeue");

        Assert.Contains("That is not a valid command.", page);
        Assert.AreEqual(HttpStatusCode.NoContent, command.StatusCode);
    }

    [TestMethod]
    [DataRow("/admin/chat-bans")]
    [DataRow("/admin/remote-commands")]
    [DataRow("/admin/player-connections")]
    public async Task OnlyAnAdministratorOpensTheLiveAdminPages(string path)
    {
        using var factory = new LiveWebFactory();
        using var administrator = factory.Client(HubWebFactory.Administrator);
        using var player = factory.Client(HubWebFactory.Player);
        using var visitor = factory.Client();

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();

        var allowed = await administrator.GetAsync(path);
        var forbidden = await player.GetAsync(path);
        var anonymous = await visitor.GetAsync(path);

        Assert.AreEqual(HttpStatusCode.OK, allowed.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, forbidden.StatusCode);
        Assert.AreEqual(HttpStatusCode.Unauthorized, anonymous.StatusCode);
    }

    [TestMethod]
    public async Task APlayerCannotQueueACommand()
    {
        using var factory = new LiveWebFactory();
        using var player = factory.Client(HubWebFactory.Player);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        var fields = new Dictionary<string, string> { ["serverId"] = credentials.Server.Id.ToString(), ["command"] = "!forcestart" };
        var queue = await player.PostAsync("/admin/remote-commands", new FormUrlEncodedContent(fields));
        var command = await server.GetAsync("/api/game-info/server-command/dequeue");

        Assert.AreEqual(HttpStatusCode.Forbidden, queue.StatusCode);
        Assert.AreEqual(HttpStatusCode.NoContent, command.StatusCode);
    }

    [TestMethod]
    public async Task TheAdminPagesOfAModuleThatIsOffAreNotFoundAndLeaveTheMenu()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();

        var on = await client.GetStringAsync("/admin");

        await factory.EnableAsync(true, false, false);

        var chatBans = await client.GetAsync("/admin/chat-bans");
        var remoteCommands = await client.GetAsync("/admin/remote-commands");
        var connections = await client.GetAsync("/admin/player-connections");
        var off = await client.GetStringAsync("/admin");

        Assert.Contains("/admin/chat-bans", on);
        Assert.Contains("/admin/remote-commands", on);
        Assert.AreEqual(HttpStatusCode.NotFound, chatBans.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, remoteCommands.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, connections.StatusCode);
        Assert.DoesNotContain("/admin/chat-bans", off);
        Assert.DoesNotContain("/admin/remote-commands", off);
        Assert.Contains("/admin/player-connections", off);
    }

    [TestMethod]
    public async Task TheAdministratorSeesTheAccountsOnTheSameAddress()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();

        await factory.RunAsync(async services =>
        {
            var connections = services.GetRequiredService<IPlayerConnectionService>();

            await connections.RegisterAsync(new PlayerConnectionCommand { CommunityId = Alice.ToString(), IpAddress = "200.100.50.1", Name = "AliceMainAccount" });
            await connections.RegisterAsync(new PlayerConnectionCommand { CommunityId = (Alice + 1).ToString(), IpAddress = "200.100.50.1", Name = "AliceSecondAccount" });
        });

        var page = await client.GetStringAsync($"/admin/player-connections?communityId={Alice}");
        var unknown = await client.GetStringAsync($"/admin/player-connections?communityId={Alice + 5}");

        Assert.Contains("AliceMainAccount", page);
        Assert.Contains("AliceSecondAccount", page);
        Assert.DoesNotContain("200.100.50.1", page);
        Assert.Contains("No connection recorded for this player.", unknown);
    }
}