using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Core.Live;

[TestClass]
public class PlayerConnectionTests
{
    private const long Alice = 76561198000000001;
    private const long Bob = 76561198000000002;
    private const long Carol = 76561198000000003;
    private const string Home = "200.100.50.1";
    private const string Office = "200.100.50.2";
    private const string Elsewhere = "8.8.8.8";

    [TestMethod]
    public async Task TheFirstConnectionHasNoRelatedPlayers()
    {
        using var hub = new TestHub();

        var related = await RegisterAsync(hub, Alice, Home, "Alice");
        var connections = await ConnectionsAsync(hub);

        Assert.IsEmpty(related!);
        Assert.HasCount(1, connections);
        Assert.AreEqual(1, connections[0].Count);
        Assert.AreEqual(hub.Clock.UtcNow, connections[0].FirstSeenAtUtc);
    }

    [TestMethod]
    public async Task PlayersOnTheSameAddressAreGroupedTogether()
    {
        using var hub = new TestHub();

        await RegisterAsync(hub, Bob, Home, "Bob");
        await RegisterAsync(hub, Bob, Office, "Bobby");
        await RegisterAsync(hub, Carol, Elsewhere, "Carol");

        hub.Clock.Advance(TimeSpan.FromDays(1));

        var related = await RegisterAsync(hub, Alice, Home, "Alice");
        var fromBob = await hub.RunAsync(services => services.GetRequiredService<IPlayerConnectionService>().RelatedAsync(Bob));
        var fromCarol = await hub.RunAsync(services => services.GetRequiredService<IPlayerConnectionService>().RelatedAsync(Carol));

        Assert.HasCount(1, related!);
        Assert.AreEqual(Bob.ToString(), related![0].CommunityId);
        Assert.AreEqual("Bobby", related[0].Name);
        Assert.AreEqual("STEAM_1:0:19867137", related[0].SteamId);
        Assert.AreEqual($"https://steamcommunity.com/profiles/{Bob}", related[0].ProfileUrl);
        Assert.HasCount(1, fromBob);
        Assert.AreEqual(Alice.ToString(), fromBob[0].CommunityId);
        Assert.IsEmpty(fromCarol);
    }

    [TestMethod]
    public async Task ConnectingAgainCountsAndKeepsTheFirstDate()
    {
        using var hub = new TestHub();

        var first = hub.Clock.UtcNow;

        await RegisterAsync(hub, Alice, Home, "Alice");
        hub.Clock.Advance(TimeSpan.FromDays(2));
        await RegisterAsync(hub, Alice, Home, "Alice!");
        await RegisterAsync(hub, Alice, Office, "Alice!");

        var details = await hub.RunAsync(services => services.GetRequiredService<IPlayerConnectionService>().DetailsAsync(Alice));
        var connections = await ConnectionsAsync(hub);

        Assert.HasCount(2, connections);
        Assert.AreEqual("Alice!", details.Player!.Name);
        Assert.AreEqual(3, details.Player.ConnectionCount);
        Assert.AreEqual(first, details.Player.FirstConnectedAtUtc);
        Assert.AreEqual(hub.Clock.UtcNow, details.Player.LastConnectedAtUtc);
        Assert.IsEmpty(details.RelatedPlayers);
    }

    [TestMethod]
    public async Task APlayerNeverSeenHasNoDetails()
    {
        using var hub = new TestHub();

        var details = await hub.RunAsync(services => services.GetRequiredService<IPlayerConnectionService>().DetailsAsync(Alice));

        Assert.IsNull(details.Player);
        Assert.IsEmpty(details.RelatedPlayers);
    }

    [TestMethod]
    [DataRow("76561198000000001", "not-an-address", "Alice")]
    [DataRow("abc", "200.100.50.1", "Alice")]
    [DataRow("123", "200.100.50.1", "Alice")]
    [DataRow("76561198000000001", "200.100.50.1", " ")]
    [DataRow(null, null, null)]
    public async Task AnInvalidConnectionIsNotRecorded(string? communityId, string? ipAddress, string? name)
    {
        using var hub = new TestHub();

        var related = await hub.RunAsync(services => services.GetRequiredService<IPlayerConnectionService>().RegisterAsync(new PlayerConnectionCommand { CommunityId = communityId, IpAddress = ipAddress, Name = name }));

        Assert.IsNull(related);
        Assert.IsEmpty(await ConnectionsAsync(hub));
    }

    [TestMethod]
    public async Task TheCleanupRemovesOnlyConnectionsOlderThanThreeMonths()
    {
        using var hub = new TestHub();

        await RegisterAsync(hub, Alice, Home, "Alice");
        await RegisterAsync(hub, Bob, Home, "Bob");

        hub.Clock.UtcNow = hub.Clock.UtcNow.AddMonths(CoreConsts.PlayerConnectionRetentionInMonths).AddDays(-1);
        await RegisterAsync(hub, Bob, Home, "Bob");

        var early = await hub.RunAsync(services => services.GetRequiredService<IPlayerConnectionService>().CleanupAsync());

        hub.Clock.Advance(TimeSpan.FromDays(2));

        var deleted = await hub.RunAsync(services => services.GetRequiredService<IPlayerConnectionService>().CleanupAsync());
        var connections = await ConnectionsAsync(hub);

        Assert.AreEqual(0, early);
        Assert.AreEqual(1, deleted);
        Assert.HasCount(1, connections);
        Assert.AreEqual(Bob, connections[0].CommunityId);
    }

    [TestMethod]
    public async Task TheCleanupFollowsTheRetentionChosenByTheAdministrator()
    {
        using var hub = new TestHub();

        await hub.RunAsync(services => services.GetRequiredService<ISettingsService>().SaveAsync(new PrivacySettings { ConnectionRetentionInMonths = 1 }));
        await RegisterAsync(hub, Alice, Home, "Alice");

        hub.Clock.UtcNow = hub.Clock.UtcNow.AddMonths(1).AddDays(1);

        var deleted = await hub.RunAsync(services => services.GetRequiredService<IPlayerConnectionService>().CleanupAsync());

        Assert.AreEqual(1, deleted);
        Assert.IsEmpty(await ConnectionsAsync(hub));
    }

    [TestMethod]
    public async Task TheCleanupHandlesMoreThanOneBatch()
    {
        using var hub = new TestHub();

        var total = CoreConsts.PlayerConnectionCleanupBatchSize * 2 + 5;

        for (var index = 1; index <= total; index++)
            await RegisterAsync(hub, Alice + index, Home, $"Player {index}");

        hub.Clock.UtcNow = hub.Clock.UtcNow.AddMonths(CoreConsts.PlayerConnectionRetentionInMonths).AddDays(1);

        var deleted = await hub.RunAsync(services => services.GetRequiredService<IPlayerConnectionService>().CleanupAsync());

        Assert.AreEqual(total, deleted);
        Assert.IsEmpty(await ConnectionsAsync(hub));
    }

    private static Task<List<PlayerConnectionResult>?> RegisterAsync(TestHub hub, long communityId, string ipAddress, string name)
    {
        return hub.RunAsync(services => services.GetRequiredService<IPlayerConnectionService>().RegisterAsync(new PlayerConnectionCommand { CommunityId = communityId.ToString(), IpAddress = ipAddress, Name = name }));
    }

    private static Task<List<PlayerConnection>> ConnectionsAsync(TestHub hub)
    {
        return hub.RunAsync(services => services.GetRequiredService<HubDbContext>().Set<PlayerConnection>().AsNoTracking().ToListAsync());
    }
}
