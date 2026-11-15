using L4D2PlayStats.Hub.Core.Modules.Live.Services;
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
