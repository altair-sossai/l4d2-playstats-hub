using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Tests.Community.Support;
using L4D2PlayStats.Hub.Tests.Support;

namespace L4D2PlayStats.Hub.Tests.Community;

[TestClass]
public class VoteInviteTests
{
    private const string Invite = "class=\"vote-invite";
    private const string CommunityPage = "/community";

    [TestMethod]
    public async Task AVisitorWhoIsNotSignedInSeesTheInvite()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var visitor = site.Factory.Client();

        Assert.Contains(Invite, await visitor.GetStringAsync(CommunityPage));
    }

    [TestMethod]
    public async Task APlayerWithVotesLeftSeesTheInviteUntilTheLastVoteOfTheDay()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, VotesPerDay = 1 });

        using var voter = site.Factory.Client(site.Voter);

        var before = await voter.GetStringAsync(CommunityPage);
        var pair = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, pair, nameof(CommunityVoteOutcome.Left));

        var after = await voter.GetStringAsync(CommunityPage);
        var home = await voter.GetStringAsync("/");

        Assert.Contains(Invite, before);
        Assert.DoesNotContain(Invite, after);
        Assert.DoesNotContain(Invite, home);
    }

    [TestMethod]
    public async Task APlayerWhoCannotVoteDoesNotSeeTheInvite()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 100 });

        using var voter = site.Factory.Client(site.Voter);

        Assert.DoesNotContain(Invite, await voter.GetStringAsync(CommunityPage));
    }

    [TestMethod]
    public async Task ABlockedPlayerDoesNotSeeTheInvite()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var voter = site.Factory.Client(site.Voter);
        using var administrator = site.Factory.Client(HubWebFactory.Administrator);

        await HubWebFactory.PostFormAsync(administrator, "/admin/community", "/admin/community/voters/block", new Dictionary<string, string> { ["communityId"] = site.Voter.ToString() });

        Assert.DoesNotContain(Invite, await voter.GetStringAsync(CommunityPage));
    }
}
