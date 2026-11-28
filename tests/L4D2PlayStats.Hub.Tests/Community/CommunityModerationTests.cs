using System.Net;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Community;
using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Tests.Community.Support;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Community;

[TestClass]
public class CommunityModerationTests
{
    private const string AdminPage = "/admin/community";
    private const string BlockedMessage = "You can no longer vote in the community ranking.";

    [TestMethod]
    public async Task ABlockedVoterSeesTheNoticeAndCannotVote()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var voter = site.Factory.Client(site.Voter);
        using var administrator = site.Factory.Client(HubWebFactory.Administrator);

        var pair = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, pair, nameof(CommunityVoteOutcome.Left));

        var next = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage));
        var response = await HubWebFactory.PostFormAsync(administrator, AdminPage, "/admin/community/voters/block", new Dictionary<string, string> { ["communityId"] = site.Voter.ToString() });
        var admin = await administrator.GetStringAsync(AdminPage);
        var ballot = await voter.GetStringAsync(CommunityWebSite.VotePage);

        if (next != null)
            await PostWithTokenAsync(voter, ballot, next, nameof(CommunityVoteOutcome.Right));

        var offers = await site.Factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Set<CommunityOffer>().CountAsync());

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("The voter was blocked and can no longer vote.", admin);
        Assert.Contains("Blocked voters", admin);
        Assert.Contains($"name=\"communityId\" value=\"{site.Voter}\"", admin);
        Assert.Contains(BlockedMessage, ballot);
        Assert.IsNull(CommunityWebSite.Pair(ballot));
        Assert.HasCount(1, await site.VotesAsync());
        Assert.AreEqual(0, offers);
    }

    [TestMethod]
    public async Task AnUnblockedVoterCanVoteAgain()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var voter = site.Factory.Client(site.Voter);
        using var administrator = site.Factory.Client(HubWebFactory.Administrator);

        await HubWebFactory.PostFormAsync(administrator, AdminPage, "/admin/community/voters/block", new Dictionary<string, string> { ["communityId"] = site.Voter.ToString() });

        var blocked = await voter.GetStringAsync(CommunityWebSite.VotePage);
        var response = await HubWebFactory.PostFormAsync(administrator, AdminPage, "/admin/community/voters/unblock", new Dictionary<string, string> { ["communityId"] = site.Voter.ToString() });
        var admin = await administrator.GetStringAsync(AdminPage);
        var ballot = await voter.GetStringAsync(CommunityWebSite.VotePage);
        var pair = CommunityWebSite.Pair(ballot)!;

        await CommunityWebSite.PostVoteAsync(voter, pair, nameof(CommunityVoteOutcome.Left));

        Assert.Contains(BlockedMessage, blocked);
        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("The voter was unblocked and can vote again.", admin);
        Assert.Contains("Nobody is blocked from voting.", admin);
        Assert.DoesNotContain(BlockedMessage, ballot);
        Assert.HasCount(1, await site.VotesAsync());
    }

    [TestMethod]
    public async Task AnAdministratorDeletesASingleVoteFromTheLatestVotes()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var voter = site.Factory.Client(site.Voter);
        using var administrator = site.Factory.Client(HubWebFactory.Administrator);

        var first = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, first, nameof(CommunityVoteOutcome.Left));

        var second = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, second, nameof(CommunityVoteOutcome.Right));

        var votes = await site.VotesAsync();
        var removed = votes[0];
        var page = await administrator.GetStringAsync("/community/votes");
        var visitor = await voter.GetStringAsync("/community/votes");
        var response = await HubWebFactory.PostFormAsync(administrator, "/community/votes", "/admin/community/votes/delete", new Dictionary<string, string> { ["voteId"] = removed.Id.ToString(), ["returnUrl"] = "/community/votes" });
        var left = await site.VotesAsync();
        var ratings = await site.RatingsAsync();

        Assert.HasCount(2, votes);
        Assert.Contains($"name=\"voteId\" value=\"{removed.Id}\"", page);
        Assert.DoesNotContain("name=\"voteId\"", visitor);
        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual("/community/votes", response.Headers.Location?.OriginalString);
        Assert.HasCount(1, left);
        Assert.AreEqual(votes[1].Id, left[0].Id);
        Assert.AreEqual(2, ratings.Sum(rating => rating.Comparisons));
    }

    [TestMethod]
    public async Task TheDeleteButtonIsOnThePlayerPageForAnAdministrator()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var voter = site.Factory.Client(site.Voter);
        using var administrator = site.Factory.Client(HubWebFactory.Administrator);

        var pair = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, pair, nameof(CommunityVoteOutcome.Left));

        var vote = (await site.VotesAsync())[0];
        var page = await administrator.GetStringAsync($"/player/{pair.Left}");
        var visitor = await voter.GetStringAsync($"/player/{pair.Left}");

        Assert.Contains($"name=\"voteId\" value=\"{vote.Id}\"", page);
        Assert.Contains($"name=\"returnUrl\" value=\"/player/{pair.Left}\"", page);
        Assert.DoesNotContain("name=\"voteId\"", visitor);
    }

    [TestMethod]
    public async Task AnOutsideReturnAddressGoesBackToTheAdministration()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var administrator = site.Factory.Client(HubWebFactory.Administrator);

        var response = await HubWebFactory.PostFormAsync(administrator, AdminPage, "/admin/community/votes/delete", new Dictionary<string, string> { ["voteId"] = "999", ["returnUrl"] = "https://example.com/" });
        var admin = await administrator.GetStringAsync(AdminPage);

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual("/admin/community?tab=voters", response.Headers.Location?.OriginalString);
        Assert.Contains("That vote no longer exists.", admin);
    }

    [TestMethod]
    public async Task OnlyAnAdministratorCanBlockOrDeleteVotes()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var voter = site.Factory.Client(site.Voter);

        var pair = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, pair, nameof(CommunityVoteOutcome.Left));

        var vote = (await site.VotesAsync())[0];

        await HubWebFactory.PostFormAsync(voter, CommunityWebSite.VotePage, "/admin/community/votes/delete", new Dictionary<string, string> { ["voteId"] = vote.Id.ToString() });
        await HubWebFactory.PostFormAsync(voter, CommunityWebSite.VotePage, "/admin/community/voters/block", new Dictionary<string, string> { ["communityId"] = pair.Left.ToString() });

        var blocks = await site.Factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Set<CommunityVoterBlock>().CountAsync());

        Assert.HasCount(1, await site.VotesAsync());
        Assert.AreEqual(0, blocks);
    }

    [TestMethod]
    public async Task BlockingCanAlsoDeleteTheVotesOfTheVoter()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var voter = site.Factory.Client(site.Voter);
        using var administrator = site.Factory.Client(HubWebFactory.Administrator);

        var pair = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, pair, nameof(CommunityVoteOutcome.Left));

        var page = await administrator.GetStringAsync("/admin/community?tab=voters");
        var response = await HubWebFactory.PostFormAsync(administrator, AdminPage, "/admin/community/voters/block", new Dictionary<string, string> { ["communityId"] = site.Voter.ToString(), ["removeVotes"] = "true", ["returnUrl"] = "/admin/community?tab=voters" });
        var admin = await administrator.GetStringAsync("/admin/community?tab=voters");
        var ballot = await voter.GetStringAsync(CommunityWebSite.VotePage);

        Assert.Contains("id=\"ca-block-modal\"", page);
        Assert.Contains($"data-block-voter=\"{site.Voter}\"", page);
        Assert.Contains($"href=\"/community/votes?voter={site.Voter}\"", page);
        Assert.AreEqual("/admin/community?tab=voters", response.Headers.Location?.OriginalString);
        Assert.Contains("The voter was blocked and can no longer vote.", admin);
        Assert.Contains("Votes removed: 1", admin);
        Assert.Contains(BlockedMessage, ballot);
        Assert.IsEmpty(await site.VotesAsync());
        Assert.IsEmpty(await site.RatingsAsync());
    }

    [TestMethod]
    [DataRow("", "ca-settings")]
    [DataRow("?tab=voters", "ca-voters")]
    [DataRow("?page=2", "ca-voters")]
    [DataRow("?tab=Maintenance", "ca-maintenance")]
    [DataRow("?tab=unknown", "ca-settings")]
    public async Task TheAddressChoosesTheOpenTab(string query, string pane)
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var administrator = site.Factory.Client(HubWebFactory.Administrator);

        var page = await administrator.GetStringAsync(AdminPage + query);

        Assert.Contains($"class=\"tab-pane active\" id=\"{pane}\"", page);
        Assert.AreEqual(1, Regex.Matches(page, "class=\"tab-pane active\"").Count);
    }

    [TestMethod]
    public async Task APlayerWhoNeverVotedCanBeBlockedFromTheBlockedTab()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var voter = site.Factory.Client(site.Voter);
        using var administrator = site.Factory.Client(HubWebFactory.Administrator);

        var tab = await administrator.GetStringAsync("/admin/community?tab=blocked");
        var response = await HubWebFactory.PostFormAsync(administrator, AdminPage, "/admin/community/voters/block", new Dictionary<string, string> { ["communityId"] = site.Voter.ToString(), ["removeVotes"] = "false", ["returnUrl"] = "/admin/community?tab=blocked" });
        var admin = await administrator.GetStringAsync("/admin/community?tab=blocked");

        Assert.Contains("data-player-picker=\"#ca-block-player\"", tab);
        Assert.Contains("id=\"ca-block-add-modal\"", tab);
        Assert.Contains("name=\"returnUrl\" value=\"/admin/community?tab=blocked\"", tab);
        Assert.AreEqual("/admin/community?tab=blocked", response.Headers.Location?.OriginalString);
        Assert.Contains("The voter was blocked and can no longer vote.", admin);
        Assert.Contains($"href=\"/community/votes?voter={site.Voter}\"", admin);
        Assert.Contains(BlockedMessage, await voter.GetStringAsync(CommunityWebSite.VotePage));
    }

    private static Task<HttpResponseMessage> PostWithTokenAsync(HttpClient client, string html, CommunityPair pair, string outcome)
    {
        var token = HubWebFactory.Token(html);

        return client.PostAsync(CommunityWebSite.VotePage, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Left"] = pair.Left.ToString(),
            ["Right"] = pair.Right.ToString(),
            ["Outcome"] = outcome,
            ["__RequestVerificationToken"] = token
        }));
    }
}
