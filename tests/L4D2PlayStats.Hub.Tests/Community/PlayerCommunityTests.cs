using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Community.Support;
using L4D2PlayStats.Hub.Tests.Support;

namespace L4D2PlayStats.Hub.Tests.Community;

[TestClass]
public class PlayerCommunityTests
{
    [TestMethod]
    public async Task ThePlayerPageShowsThePositionAndTheVotesAboutAndByThePlayer()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, MinimumComparisonsToBeListed = 1, MinimumMatchesToBeCandidate = 0 });

        using var voter = site.Factory.Client(site.Voter);
        using var visitor = site.Factory.Client();

        var pair = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, pair, nameof(CommunityVoteOutcome.Left));

        var compared = await visitor.GetStringAsync($"/player/{pair.Left}");
        var votedBy = await visitor.GetStringAsync($"/player/{site.Voter}");

        Assert.Contains("class=\"player-community\"", compared);
        Assert.Contains("#1", compared);
        Assert.Contains("href=\"#votes-received\"", compared);
        Assert.Contains($"/avatar/{site.Voter}\"", compared);
        Assert.Contains($"href=\"/community/votes?player={pair.Left}\"", compared);
        Assert.Contains("href=\"#votes-cast\"", votedBy);
        Assert.Contains($"/avatar/{pair.Left}\"", votedBy);
        Assert.Contains($"href=\"/community/votes?voter={site.Voter}\"", votedBy);
    }

    [TestMethod]
    public async Task HiddenVotersAreNotShownOnThePlayerPage()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, MinimumComparisonsToBeListed = 1, MinimumMatchesToBeCandidate = 0, ShowVoterNames = false });

        using var voter = site.Factory.Client(site.Voter);
        using var visitor = site.Factory.Client();

        var pair = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, pair, nameof(CommunityVoteOutcome.Right));

        var compared = await visitor.GetStringAsync($"/player/{pair.Right}");
        var votedBy = await visitor.GetStringAsync($"/player/{site.Voter}");

        Assert.Contains("href=\"#votes-received\"", compared);
        Assert.DoesNotContain($"/avatar/{site.Voter}\"", compared);
        Assert.Contains("A player voted", compared);
        Assert.DoesNotContain("href=\"#votes-cast\"", votedBy);
        Assert.DoesNotContain($"voter={site.Voter}", votedBy);
    }

    [TestMethod]
    public async Task ThePlayerPageHasNoCommunityPartWhileTheModuleIsOff()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 }, false);

        using var visitor = site.Factory.Client();

        var page = await visitor.GetStringAsync($"/player/{site.Voter}");

        Assert.DoesNotContain("class=\"player-community", page);
        Assert.DoesNotContain("href=\"#votes-received\"", page);
    }

    [TestMethod]
    public async Task APlayerWhoNeverTookPartShowsNoCommunityPart()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var visitor = site.Factory.Client();

        var page = await visitor.GetStringAsync("/player/76561197960265729");

        Assert.Contains("Open Steam profile", page);
        Assert.DoesNotContain("class=\"player-community", page);
        Assert.DoesNotContain("href=\"#votes-received\"", page);
    }

    [TestMethod]
    public async Task AComparisonShowsBothPositionsButNoVoteTabs()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { Ranking = true, CommunityRanking = true });

        var credentials = await factory.CreateServerAsync();

        using (var server = factory.ServerClient(credentials))
        {
            await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", CommunityTestHub.Night, 1, 300, 200));
            await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", CommunityTestHub.Night.AddMinutes(15), 2, 600, 500));
        }

        using var visitor = factory.Client();

        var opponent = StatisticsBuilder.Sample().Survivors('B')[0];
        var page = await visitor.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}/{opponent}?period=all-time");

        Assert.Contains("Remove comparison", page);
        Assert.AreEqual(2, page.Split("<a class=\"player-community").Length - 1);
        Assert.DoesNotContain("href=\"#votes-received\"", page);
    }
}
