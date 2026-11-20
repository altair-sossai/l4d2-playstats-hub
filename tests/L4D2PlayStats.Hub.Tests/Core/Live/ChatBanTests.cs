using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Core.Live;

[TestClass]
public class ChatBanTests
{
    private const long Alice = 76561198000000001;
    private const long Admin = 76561198000000100;

    [TestMethod]
    public async Task ABanRecordsWhoCreatedItAndWhen()
    {
        using var hub = new TestHub();

        await hub.RunAsync(services => services.GetRequiredService<IChatBanService>().SaveAsync(Alice, " Alice ", " Spam ", Admin, "Admin"));

        var ban = await hub.RunAsync(services => services.GetRequiredService<IChatBanService>().FindAsync(Alice));

        Assert.AreEqual("Alice", ban!.Name);
        Assert.AreEqual("Spam", ban.Reason);
        Assert.AreEqual(Admin, ban.CreatedByCommunityId);
        Assert.AreEqual("Admin", ban.CreatedByName);
        Assert.AreEqual(hub.Clock.UtcNow, ban.CreatedAtUtc);
        Assert.IsNull(ban.UpdatedAtUtc);
    }

    [TestMethod]
    public async Task ABanWithoutANameTakesTheNameTheSiteKnows()
    {
        using var hub = new TestHub();

        const long bob = 76561198000000002;

        await hub.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();

            dbContext.PlayerProfiles.Add(new PlayerProfile { CommunityId = Alice, Name = "Alice", FirstSeenAtUtc = hub.Clock.UtcNow, LastSeenAtUtc = hub.Clock.UtcNow });

            return await dbContext.SaveChangesAsync();
        });

        await hub.RunAsync(services => services.GetRequiredService<IPlayerConnectionService>().RegisterAsync(new PlayerConnectionCommand { CommunityId = bob.ToString(), IpAddress = "10.0.0.2", Name = "Bob" }));
        await hub.RunAsync(services => services.GetRequiredService<IChatBanService>().SaveAsync(Alice, null, "Spam", Admin, "Admin"));
        await hub.RunAsync(services => services.GetRequiredService<IChatBanService>().SaveAsync(bob, " ", "Spam", Admin, "Admin"));
        await hub.RunAsync(services => services.GetRequiredService<IChatBanService>().SaveAsync(76561198000000003, null, "Spam", Admin, "Admin"));

        var bans = await hub.RunAsync(services => services.GetRequiredService<IChatBanService>().ListAsync());

        Assert.AreEqual("Alice", bans.Single(ban => ban.CommunityId == Alice).Name);
        Assert.AreEqual("Bob", bans.Single(ban => ban.CommunityId == bob).Name);
        Assert.IsNull(bans.Single(ban => ban.CommunityId == 76561198000000003).Name);
    }

    [TestMethod]
    public async Task BanningAgainUpdatesTheReasonAndKeepsOneBan()
    {
        using var hub = new TestHub();

        await hub.RunAsync(services => services.GetRequiredService<IChatBanService>().SaveAsync(Alice, "Alice", "Spam", Admin, "Admin"));

        var created = hub.Clock.UtcNow;
        hub.Clock.Advance(TimeSpan.FromDays(1));

        await hub.RunAsync(services => services.GetRequiredService<IChatBanService>().SaveAsync(Alice, null, "Insults", Admin, "Admin"));

        var bans = await hub.RunAsync(services => services.GetRequiredService<IChatBanService>().ListAsync());

        Assert.HasCount(1, bans);
        Assert.AreEqual("Alice", bans[0].Name);
        Assert.AreEqual("Insults", bans[0].Reason);
        Assert.AreEqual(created, bans[0].CreatedAtUtc);
        Assert.AreEqual(hub.Clock.UtcNow, bans[0].UpdatedAtUtc);
    }

    [TestMethod]
    public async Task ABanCanBeRemoved()
    {
        using var hub = new TestHub();

        await hub.RunAsync(services => services.GetRequiredService<IChatBanService>().SaveAsync(Alice, "Alice", null, Admin, "Admin"));

        var removed = await hub.RunAsync(services => services.GetRequiredService<IChatBanService>().RemoveAsync(Alice));
        var again = await hub.RunAsync(services => services.GetRequiredService<IChatBanService>().RemoveAsync(Alice));
        var bans = await hub.RunAsync(services => services.GetRequiredService<IChatBanService>().ListAsync());

        Assert.IsTrue(removed);
        Assert.IsFalse(again);
        Assert.IsEmpty(bans);
    }

    [TestMethod]
    public async Task AnInvalidSteamIdIsNotBanned()
    {
        using var hub = new TestHub();

        var ban = await hub.RunAsync(services => services.GetRequiredService<IChatBanService>().SaveAsync(123, "Nobody", null, Admin, "Admin"));
        var bans = await hub.RunAsync(services => services.GetRequiredService<IChatBanService>().ListAsync());

        Assert.IsNull(ban);
        Assert.IsEmpty(bans);
    }
}