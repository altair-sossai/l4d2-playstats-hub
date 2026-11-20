using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Tests.Community.Support;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class InGameBrowserTests
{
    private const string Invite = "class=\"vote-invite";

    [TestMethod]
    public async Task TheSiteShowsTheVoteInvite()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var client = site.Factory.Client();

        Assert.Contains(Invite, await client.GetStringAsync("/community"));
    }

    [TestMethod]
    public async Task TheGameBrowserNeverShowsTheVoteInvite()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var client = site.Factory.Client();

        Assert.DoesNotContain(Invite, await client.GetStringAsync("/?steamid=76561198000000001"));
        Assert.DoesNotContain(Invite, await client.GetStringAsync("/community"));
    }
}
