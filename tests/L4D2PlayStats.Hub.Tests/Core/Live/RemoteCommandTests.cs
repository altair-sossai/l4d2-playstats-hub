using L4D2PlayStats.Hub.Core.Modules.Administrators.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Live.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Core.Live;

[TestClass]
public class RemoteCommandTests
{
    private const long Alice = 76561198000000001;
    private const long Admin = 76561198000000100;

    [TestMethod]
    [DataRow("sm_forcestart", "sm_forcestart")]
    [DataRow("!forcestart", "sm_forcestart")]
    [DataRow("/kick Alice being rude", "sm_kick Alice being rude")]
    [DataRow("  sm_map c8m1_apartment  ", "sm_map c8m1_apartment")]
    public void ACommandIsAlwaysSentAsASourceModCommand(string text, string expected)
    {
        var command = ServerCommand.Parse(text);

        Assert.AreEqual(expected, command!.FullCommand);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("hello")]
    [DataRow("quit")]
    [DataRow("rcon_password secret")]
    [DataRow("sm_say hi; quit")]
    [DataRow("sm_say hi\nquit")]
    public void AnythingElseIsNotACommand(string? text)
    {
        Assert.IsNull(ServerCommand.Parse(text));
    }

    [TestMethod]
    public void ACommandThatIsTooLongIsRefused()
    {
        Assert.IsNull(ServerCommand.Parse("sm_say " + new string('a', CoreConsts.ServerCommandMaxLength)));
    }

    [TestMethod]
    public async Task AnAdministratorQueuesACommandForOneServer()
    {
        using var hub = await HubAsync(true);

        var first = await hub.CreateServerAsync("First");
        var second = await hub.CreateServerAsync("Second");

        var result = await QueueAsync(hub, first.Server, Admin, "!forcestart");

        var fromSecond = await DequeueAsync(hub, second.Server);
        var fromFirst = await DequeueAsync(hub, first.Server);
        var again = await DequeueAsync(hub, first.Server);

        Assert.AreEqual(RemoteCommandStatus.Queued, result.Status);
        Assert.IsNull(fromSecond);
        Assert.AreEqual("sm_forcestart", fromFirst!.FullCommand);
        Assert.IsNull(again);
    }

    [TestMethod]
    public async Task CommandsLeaveTheQueueInTheOrderTheyArrived()
    {
        using var hub = await HubAsync(true);

        var credentials = await hub.CreateServerAsync();

        await QueueAsync(hub, credentials.Server, Admin, "sm_one");
        await QueueAsync(hub, credentials.Server, Admin, "sm_two");

        var first = await DequeueAsync(hub, credentials.Server);
        var second = await DequeueAsync(hub, credentials.Server);

        Assert.AreEqual("sm_one", first!.FullCommand);
        Assert.AreEqual("sm_two", second!.FullCommand);
    }

    [TestMethod]
    public async Task SomeoneWhoIsNotAnAdministratorQueuesNothing()
    {
        using var hub = await HubAsync(true);

        var credentials = await hub.CreateServerAsync();

        var result = await QueueAsync(hub, credentials.Server, Alice, "sm_forcestart");

        Assert.AreEqual(RemoteCommandStatus.NotAdministrator, result.Status);
        Assert.IsNull(await DequeueAsync(hub, credentials.Server));
        Assert.IsEmpty(await LogsAsync(hub));
    }

    [TestMethod]
    public async Task NothingIsQueuedWhileTheModuleIsOff()
    {
        using var hub = await HubAsync(false);

        var credentials = await hub.CreateServerAsync();

        var result = await QueueAsync(hub, credentials.Server, Admin, "sm_forcestart");

        Assert.AreEqual(RemoteCommandStatus.ModuleDisabled, result.Status);
        Assert.IsEmpty(await LogsAsync(hub));
    }

    [TestMethod]
    public async Task ACommandQueuedBeforeTheModuleWasTurnedOffIsNotDelivered()
    {
        using var hub = await HubAsync(true);

        var credentials = await hub.CreateServerAsync();

        await QueueAsync(hub, credentials.Server, Admin, "sm_forcestart");
        await hub.RunAsync(services => services.GetRequiredService<ISettingsService>().SaveAsync(new ModuleSettings { RemoteCommands = false }));

        Assert.IsNull(await DequeueAsync(hub, credentials.Server));
    }

    [TestMethod]
    public async Task AnInvalidCommandIsNeitherQueuedNorLogged()
    {
        using var hub = await HubAsync(true);

        var credentials = await hub.CreateServerAsync();

        var result = await QueueAsync(hub, credentials.Server, Admin, "quit");

        Assert.AreEqual(RemoteCommandStatus.InvalidCommand, result.Status);
        Assert.IsEmpty(await LogsAsync(hub));
    }

    [TestMethod]
    public async Task EveryCommandIsWrittenToTheAuditLog()
    {
        using var hub = await HubAsync(true);

        var credentials = await hub.CreateServerAsync("Zone #1");
        var queuedAt = hub.Clock.UtcNow;

        await QueueAsync(hub, credentials.Server, Admin, "!kick Alice");

        var pending = await LogsAsync(hub);

        hub.Clock.Advance(TimeSpan.FromSeconds(5));
        await DequeueAsync(hub, credentials.Server);

        var delivered = await LogsAsync(hub);

        Assert.HasCount(1, pending);
        Assert.AreEqual(Admin, pending[0].AdministratorCommunityId);
        Assert.AreEqual("Admin", pending[0].AdministratorName);
        Assert.AreEqual(credentials.Server.Id, pending[0].GameServerId);
        Assert.AreEqual("Zone #1", pending[0].GameServerName);
        Assert.AreEqual("sm_kick Alice", pending[0].Command);
        Assert.AreEqual(queuedAt, pending[0].CreatedAtUtc);
        Assert.IsNull(pending[0].DeliveredAtUtc);
        Assert.AreEqual(hub.Clock.UtcNow, delivered[0].DeliveredAtUtc);
    }

    [TestMethod]
    public async Task ACommandTheServerDidNotFetchInTimeIsDropped()
    {
        using var hub = await HubAsync(true);

        var credentials = await hub.CreateServerAsync();

        await QueueAsync(hub, credentials.Server, Admin, "sm_forcestart");

        hub.Clock.Advance(TimeSpan.FromMinutes(CoreConsts.RemoteCommandExpirationInMinutes));

        var command = await DequeueAsync(hub, credentials.Server);
        var logs = await LogsAsync(hub);

        Assert.IsNull(command);
        Assert.IsNull(logs[0].DeliveredAtUtc);
    }

    [TestMethod]
    public async Task ACommandWrittenInTheChatByAnAdministratorIsQueued()
    {
        using var hub = await HubAsync(true);

        var credentials = await hub.CreateServerAsync();

        await SendAsync(hub, credentials.Server, Alice, "!forcestart");
        await SendAsync(hub, credentials.Server, Admin, "hello");
        await SendAsync(hub, credentials.Server, Admin, "!forcestart");

        var command = await DequeueAsync(hub, credentials.Server);
        var logs = await LogsAsync(hub);

        Assert.AreEqual("sm_forcestart", command!.FullCommand);
        Assert.HasCount(1, logs);
        Assert.AreEqual(Admin, logs[0].AdministratorCommunityId);
    }

    private static async Task<TestHub> HubAsync(bool remoteCommands)
    {
        var hub = new TestHub();

        await hub.RunAsync(async services =>
        {
            await services.GetRequiredService<ISettingsService>().SaveAsync(new ModuleSettings { SiteChat = true, RemoteCommands = remoteCommands });
            await services.GetRequiredService<IAdministratorService>().AddAsync(Admin, "Admin");
        });

        return hub;
    }

    private static Task<RemoteCommandResult> QueueAsync(TestHub hub, GameServer server, long communityId, string command)
    {
        return hub.RunAsync(services => services.GetRequiredService<IRemoteCommandService>().QueueAsync(server, communityId, communityId == Admin ? "Admin" : "Alice", command));
    }

    private static Task<ServerCommand?> DequeueAsync(TestHub hub, GameServer server)
    {
        return hub.RunAsync(services => services.GetRequiredService<IRemoteCommandService>().DequeueAsync(server));
    }

    private static async Task<List<RemoteCommandLog>> LogsAsync(TestHub hub)
    {
        var page = await hub.RunAsync(services => services.GetRequiredService<IRemoteCommandService>().PageAsync(1, 100));

        return [.. page.Items];
    }

    private static Task<SiteChatResult> SendAsync(TestHub hub, GameServer server, long communityId, string message)
    {
        return hub.RunAsync(services => services.GetRequiredService<ISiteChatService>().SendAsync(server, new SiteChatUser(communityId, "Someone"), message));
    }
}
