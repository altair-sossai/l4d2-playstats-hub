using System.Net;
using System.Text.Json;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Community;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Community.Support;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Web.Live;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class RelatedAccountsTests
{
    private const string Api = "/api/admin/players/related";
    private const long Main = 76561198000000011;
    private const long Alternate = 76561198000000012;
    private const long Second = 76561198000000013;
    private const long Stranger = 76561198000000019;
    private const string SharedAddress = "200.100.50.1";

    [TestMethod]
    public async Task TheAdministratorSeesTheOtherAccountsOfTheSameAddressAndWhichAreAlreadyOnTheList()
    {
        using var factory = new LiveWebFactory();
        using var administrator = factory.Client(HubWebFactory.Administrator);
        using var player = factory.Client(HubWebFactory.Player);

        await factory.CompleteSetupAsync();
        await SeedAsync(factory);
        await factory.RunAsync(services => services.GetRequiredService<IChatBanService>().SaveAsync(Second, "Second", null, HubWebFactory.Administrator, "Admin"));

        var related = await JsonAsync(administrator, $"{Api}?id={SteamIds.SteamId(Main)}&list=chat-bans");
        var otherList = await JsonAsync(administrator, $"{Api}?id={Main}&list=forced-names");
        var alone = await JsonAsync(administrator, $"{Api}?id={Stranger}&list=chat-bans");
        var invalid = await administrator.GetAsync($"{Api}?id=12&list=chat-bans");
        var asPlayer = await player.GetAsync($"{Api}?id={Main}&list=chat-bans");

        var accounts = related.GetProperty("accounts").EnumerateArray().ToList();

        Assert.AreEqual(Main.ToString(), related.GetProperty("communityId").GetString());
        Assert.HasCount(2, accounts);
        Assert.AreEqual(Second.ToString(), accounts[0].GetProperty("communityId").GetString());
        Assert.IsTrue(accounts[0].GetProperty("linked").GetBoolean());
        Assert.AreEqual(Alternate.ToString(), accounts[1].GetProperty("communityId").GetString());
        Assert.AreEqual("VorksAlt", accounts[1].GetProperty("name").GetString());
        Assert.AreEqual(3, accounts[1].GetProperty("connections").GetInt32());
        Assert.IsFalse(accounts[1].GetProperty("linked").GetBoolean());
        Assert.IsFalse(otherList.GetProperty("accounts").EnumerateArray().Any(account => account.GetProperty("linked").GetBoolean()));
        Assert.AreEqual(0, alone.GetProperty("accounts").GetArrayLength());
        Assert.AreEqual(HttpStatusCode.NotFound, invalid.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, asPlayer.StatusCode);
    }

    [TestMethod]
    public async Task AForcedNameAlsoGoesToTheChosenRelatedAccountsOnly()
    {
        using var factory = new HubWebFactory();
        using var administrator = factory.Client(HubWebFactory.Administrator);
        const string page = "/admin/forced-names";

        await factory.CompleteSetupAsync();
        await SeedAsync(factory);

        var form = await administrator.GetStringAsync(page);
        var response = await HubWebFactory.PostFormAsync(administrator, page, $"{page}/save", new Dictionary<string, string>
        {
            ["communityId"] = Main.ToString(),
            ["name"] = "Vorks",
            ["relatedIds[0]"] = Alternate.ToString(),
            ["relatedIds[1]"] = Stranger.ToString(),
            ["returnUrl"] = page
        });
        var list = await administrator.GetStringAsync(page);
        var names = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().PlayerProfiles.AsNoTracking().Where(entry => entry.ForcedName != null).ToDictionaryAsync(entry => entry.CommunityId, entry => entry.ForcedName!));

        Assert.Contains("data-related-accounts=\"forced-names\" data-related-accounts-player=\"#fn-player\"", form);
        Assert.Contains("id=\"ra-modal\"", form);
        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("Related accounts included: 1.", list);
        Assert.HasCount(2, names);
        Assert.AreEqual("Vorks", names[Main]);
        Assert.AreEqual("Vorks", names[Alternate]);
    }

    [TestMethod]
    public async Task BlockingAVoterAlsoBlocksTheChosenRelatedAccounts()
    {
        using var site = new CommunityWebSite();
        const string page = "/admin/community";

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });
        await SeedAsync(site.Factory);

        using var administrator = site.Factory.Client(HubWebFactory.Administrator);

        var form = await administrator.GetStringAsync(page);
        var response = await HubWebFactory.PostFormAsync(administrator, page, "/admin/community/voters/block", new Dictionary<string, string>
        {
            ["communityId"] = Main.ToString(),
            ["relatedIds[0]"] = Alternate.ToString(),
            ["relatedIds[1]"] = Stranger.ToString(),
            ["returnUrl"] = page
        });
        var admin = await administrator.GetStringAsync(page);
        var blocked = await site.Factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Set<CommunityVoterBlock>().Select(block => block.CommunityId).OrderBy(id => id).ToListAsync());

        Assert.Contains("data-related-accounts=\"voter-blocks\" data-related-accounts-player=\"#ca-block-id\"", form);
        Assert.Contains("data-related-accounts=\"voter-blocks\" data-related-accounts-player=\"#ca-block-player\"", form);
        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("The voter was blocked and can no longer vote.", admin);
        Assert.Contains("Related accounts included: 1.", admin);
        CollectionAssert.AreEqual(new[] { Main, Alternate }, blocked);
    }

    [TestMethod]
    public async Task TheChatBansAndTheAntiCheatListsAskAboutTheRelatedAccounts()
    {
        using var factory = new LiveWebFactory();
        using var administrator = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { LiveServer = true, SiteChat = true, AntiCheat = true });

        var chatBans = await administrator.GetStringAsync("/admin/chat-bans");
        var antiCheat = await administrator.GetStringAsync("/admin/anti-cheat");

        Assert.Contains("id=\"ra-modal\"", chatBans);
        Assert.Contains("\"relatedList\": \"chat-bans\"", chatBans);
        Assert.Contains("id=\"ra-modal\"", antiCheat);
        Assert.Contains("\"relatedList\": \"anti-cheat-required\"", antiCheat);
    }

    private static async Task<JsonElement> JsonAsync(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
    }

    private static Task SeedAsync(HubWebFactory factory)
    {
        return factory.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();
            var now = DateTime.UtcNow;

            dbContext.Set<PlayerConnection>().Add(new PlayerConnection { IpAddress = SharedAddress, CommunityId = Main, Name = "Vorks", FirstSeenAtUtc = now.AddDays(-30), LastSeenAtUtc = now.AddDays(-1), Count = 10 });
            dbContext.Set<PlayerConnection>().Add(new PlayerConnection { IpAddress = SharedAddress, CommunityId = Alternate, Name = "VorksAlt", FirstSeenAtUtc = now.AddDays(-20), LastSeenAtUtc = now.AddDays(-5), Count = 3 });
            dbContext.Set<PlayerConnection>().Add(new PlayerConnection { IpAddress = SharedAddress, CommunityId = Second, Name = "VorksSecond", FirstSeenAtUtc = now.AddDays(-10), LastSeenAtUtc = now.AddDays(-2), Count = 1 });
            dbContext.Set<PlayerConnection>().Add(new PlayerConnection { IpAddress = "200.100.50.9", CommunityId = Stranger, Name = "Stranger", FirstSeenAtUtc = now.AddDays(-10), LastSeenAtUtc = now.AddDays(-2), Count = 1 });

            await dbContext.SaveChangesAsync();
        });
    }
}
