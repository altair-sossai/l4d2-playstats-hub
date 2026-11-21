using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Community;
using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Tests.Community.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Community;

[TestClass]
public class CommunityRankingServiceTests
{
    private const int EnoughVotes = 1000;

    private static readonly CommunityVoteOutcome[] Outcomes =
    [
        CommunityVoteOutcome.Left, CommunityVoteOutcome.Right, CommunityVoteOutcome.Left, CommunityVoteOutcome.SameLevel,
        CommunityVoteOutcome.UnknownLeft, CommunityVoteOutcome.Right, CommunityVoteOutcome.Left, CommunityVoteOutcome.UnknownBoth,
        CommunityVoteOutcome.SameLevel, CommunityVoteOutcome.Left, CommunityVoteOutcome.UnknownRight
    ];

    [TestMethod]
    public async Task RebuildingGivesExactlyWhatVotingGave()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, VotesPerDay = EnoughVotes });

        var votes = await VoteAsync(community, community.Everyone);
        var incremental = CommunityTestHub.Snapshot(await community.RatingsAsync());
        var replayed = await community.Hub.RunAsync(services => services.GetRequiredService<ICommunityRankingService>().RebuildAsync());
        var rebuilt = CommunityTestHub.Snapshot(await community.RatingsAsync());

        Assert.IsGreaterThan(60, votes);
        Assert.AreEqual(votes, replayed);
        Assert.AreEqual(incremental, rebuilt);
    }

    [TestMethod]
    public async Task RebuildingFromNothingGivesTheSameRatings()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, VotesPerDay = EnoughVotes });
        await VoteAsync(community, community.Everyone);

        var incremental = CommunityTestHub.Snapshot(await community.RatingsAsync());

        await community.Hub.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();

            dbContext.RemoveRange(await dbContext.Set<CommunityRating>().ToListAsync());

            await dbContext.SaveChangesAsync();
        });

        var emptied = await community.RatingsAsync();

        await community.Hub.RunAsync(services => services.GetRequiredService<ICommunityRankingService>().RebuildAsync());

        var rebuilt = CommunityTestHub.Snapshot(await community.RatingsAsync());

        Assert.IsEmpty(emptied);
        Assert.AreEqual(incremental, rebuilt);
    }

    [TestMethod]
    public async Task RebuildingAppliesTheCurrentSettingsToEveryVote()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        var pair = (await community.AnswerAsync(community.Voter, CommunityVoteOutcome.Left))!;

        await community.SaveAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, InitialRating = 1000 });

        var untouched = (await community.RatingsAsync()).ToDictionary(r => r.CommunityId);

        await community.Hub.RunAsync(services => services.GetRequiredService<ICommunityRankingService>().RebuildAsync());

        var rebuilt = (await community.RatingsAsync()).ToDictionary(r => r.CommunityId);

        Assert.IsGreaterThan(1500d, untouched[pair.Left].Rating);
        Assert.AreEqual(untouched[pair.Left].Rating - 1500, rebuilt[pair.Left].Rating - 1000, 0.001);
        Assert.AreEqual(2000, rebuilt[pair.Left].Rating + rebuilt[pair.Right].Rating, 0.001);
    }

    [TestMethod]
    public async Task RemovingAVoterTakesTheirVotesOutOfTheRatings()
    {
        using var community = new CommunityTestHub();

        var honest = community.TeamA.Take(2).ToList();
        var abuser = community.TeamB[0];

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, VotesPerDay = EnoughVotes });
        await VoteAsync(community, honest);

        var before = CommunityTestHub.Snapshot(await community.RatingsAsync());
        var honestVotes = (await community.VotesAsync()).Count;

        var abuserPairs = await community.AnswerEverythingAsync(abuser, CommunityVoteOutcome.Left);
        var during = CommunityTestHub.Snapshot(await community.RatingsAsync());
        var removed = await community.Hub.RunAsync(services => services.GetRequiredService<ICommunityRankingService>().RemoveVoterAsync(abuser));
        var after = CommunityTestHub.Snapshot(await community.RatingsAsync());
        var votes = await community.VotesAsync();

        Assert.HasCount(21, abuserPairs);
        Assert.AreEqual(21, removed);
        Assert.AreNotEqual(before, during);
        Assert.AreEqual(before, after);
        Assert.HasCount(honestVotes, votes);
        Assert.IsFalse(votes.Any(vote => vote.VoterId == abuser));
    }

    [TestMethod]
    public async Task ARemovedVoterStartsOver()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, VotesPerDay = 2 });
        await community.AnswerAsync(community.Voter, CommunityVoteOutcome.UnknownLeft);
        await community.AnswerAsync(community.Voter, CommunityVoteOutcome.Left);
        await community.AnswerAsync(community.Voter, CommunityVoteOutcome.Left);

        var limited = await community.BallotAsync(community.Voter);

        await community.Hub.RunAsync(services => services.GetRequiredService<ICommunityRankingService>().RemoveVoterAsync(community.Voter));

        var ratings = await community.RatingsAsync();
        var fresh = await community.BallotAsync(community.Voter);
        var offers = await community.Hub.RunAsync(services => services.GetRequiredService<HubDbContext>().Set<CommunityOffer>().CountAsync());

        Assert.AreEqual(CommunityBallotStatus.LimitReached, limited.Status);
        Assert.IsEmpty(ratings);
        Assert.AreEqual(CommunityBallotStatus.Ready, fresh.Status);
        Assert.AreEqual(2, fresh.VotesLeft);
        Assert.AreEqual(1, offers);
    }

    [TestMethod]
    public async Task RemovingSomeoneWhoNeverVotedChangesNothing()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });
        await community.AnswerAsync(community.Voter, CommunityVoteOutcome.Left);

        var before = CommunityTestHub.Snapshot(await community.RatingsAsync());
        var removed = await community.Hub.RunAsync(services => services.GetRequiredService<ICommunityRankingService>().RemoveVoterAsync(community.TeamB[0]));
        var after = CommunityTestHub.Snapshot(await community.RatingsAsync());

        Assert.AreEqual(0, removed);
        Assert.AreEqual(before, after);
    }

    [TestMethod]
    public async Task APlayerIsListedOnlyAfterEnoughComparisons()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, MinimumComparisonsToBeListed = 2, VotesPerDay = EnoughVotes });

        var first = (await community.AnswerAsync(community.Voter, CommunityVoteOutcome.Left))!;
        var afterOne = await community.RankingAsync();

        await community.AnswerEverythingAsync(community.Voter, CommunityVoteOutcome.SameLevel);

        var afterAll = await community.RankingAsync();
        var ratings = await community.RatingsAsync();

        Assert.IsEmpty(afterOne);
        Assert.HasCount(7, afterAll);
        Assert.IsTrue(afterAll.All(entry => entry.Comparisons >= 2));
        Assert.AreEqual(first.Left, afterAll[0].CommunityId);
        Assert.HasCount(7, ratings);
    }

    [TestMethod]
    public async Task ByDefaultFiveComparisonsAreNeededToBeListed()
    {
        using var community = new CommunityTestHub();

        await community.PlayAsync(CommunityTestHub.Night);
        await community.SaveAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, VotesPerDay = EnoughVotes });

        var pairs = await community.AnswerEverythingAsync(community.Voter, CommunityVoteOutcome.SameLevel);
        var ranking = await community.RankingAsync();
        var ratings = await community.RatingsAsync();

        Assert.HasCount(21, pairs);
        Assert.IsTrue(ratings.All(rating => rating.Comparisons == 6));
        Assert.HasCount(7, ranking);
        Assert.AreEqual(5, new CommunityRankingSettings().MinimumComparisonsToBeListed);
    }

    [TestMethod]
    public async Task TheLatestVotesShowOnlyJudgingAnswersFromTheNewest()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, VotesPerDay = EnoughVotes });

        var left = (await community.AnswerAsync(community.Voter, CommunityVoteOutcome.Left))!;
        await community.AnswerAsync(community.Voter, CommunityVoteOutcome.UnknownBoth);
        var same = (await community.AnswerAsync(community.Voter, CommunityVoteOutcome.SameLevel))!;
        var right = (await community.AnswerAsync(community.Voter, CommunityVoteOutcome.Right))!;

        var latest = await community.Hub.RunAsync(services => services.GetRequiredService<ICommunityRankingService>().LatestVotesAsync(null, null, 0, 10));
        var paged = await community.Hub.RunAsync(services => services.GetRequiredService<ICommunityRankingService>().LatestVotesAsync(null, null, 1, 1));
        var ofPlayer = await community.Hub.RunAsync(services => services.GetRequiredService<ICommunityRankingService>().LatestVotesAsync(null, left.Left, 0, 10));

        CollectionAssert.AreEqual(new[] { CommunityVoteOutcome.Right, CommunityVoteOutcome.SameLevel, CommunityVoteOutcome.Left }, latest.Select(vote => vote.Outcome).ToArray());
        Assert.AreEqual(right.Left, latest[0].LeftPlayerId);
        Assert.AreEqual(same.Right, latest[1].RightPlayerId);
        Assert.IsTrue(latest.All(vote => vote.VoterId == community.Voter && !string.IsNullOrEmpty(vote.VoterName) && !string.IsNullOrEmpty(vote.LeftName)));
        Assert.HasCount(1, paged);
        Assert.AreEqual(CommunityVoteOutcome.SameLevel, paged[0].Outcome);
        Assert.IsTrue(ofPlayer.Count >= 1 && ofPlayer.All(vote => vote.LeftPlayerId == left.Left || vote.RightPlayerId == left.Left));
    }

    [TestMethod]
    public async Task TheRankingShowsAtMostTheMaximumNumberOfPlayers()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, MinimumComparisonsToBeListed = 1, VotesPerDay = EnoughVotes });
        await community.AnswerEverythingAsync(community.Voter, CommunityVoteOutcome.Left);

        var everyone = await community.RankingAsync();

        await community.SaveAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, MinimumComparisonsToBeListed = 1, MaximumPlayersListed = 3 });

        var limited = await community.RankingAsync();

        await community.SaveAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, MinimumComparisonsToBeListed = 1, MaximumPlayersListed = 0 });

        var unlimited = await community.RankingAsync();

        Assert.HasCount(7, everyone);
        CollectionAssert.AreEqual(everyone.Take(3).Select(entry => entry.CommunityId).ToList(), limited.Select(entry => entry.CommunityId).ToList());
        Assert.HasCount(7, unlimited);
        Assert.AreEqual(100, new CommunityRankingSettings().MaximumPlayersListed);
    }

    [TestMethod]
    public async Task PlayersWithTooFewMatchesAreNotListed()
    {
        using var community = new CommunityTestHub();

        var newcomers = community.TeamB.Take(2).ToArray();

        await community.PlayAsync(CommunityTestHub.Night);
        await community.PlayAsync(CommunityTestHub.Night.AddDays(-1), newcomers);
        await community.SaveAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, MinimumComparisonsToBeListed = 1, VotesPerDay = EnoughVotes });
        await community.AnswerEverythingAsync(community.Voter, CommunityVoteOutcome.Left);

        var before = await community.RankingAsync();

        await community.SaveAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, MinimumComparisonsToBeListed = 1, MinimumMatchesToBeCandidate = 2 });

        var after = await community.RankingAsync();

        Assert.HasCount(7, before);
        Assert.HasCount(5, after);
        Assert.IsFalse(after.Any(entry => newcomers.Contains(entry.CommunityId)));
        CollectionAssert.AreEqual(Enumerable.Range(1, 5).ToList(), after.Select(entry => entry.Position).ToList());
    }

    [TestMethod]
    public async Task TheRankingGoesFromTheSurestRatingToTheLowestWithNames()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, MinimumComparisonsToBeListed = 1, VotesPerDay = EnoughVotes });
        await VoteAsync(community, community.Everyone);

        var ranking = await community.RankingAsync();
        var profiles = await community.Hub.RunAsync(services => services.GetRequiredService<HubDbContext>().PlayerProfiles.AsNoTracking().ToDictionaryAsync(p => p.CommunityId, p => p.Name));

        Assert.HasCount(8, ranking);
        CollectionAssert.AreEqual(Enumerable.Range(1, 8).ToList(), ranking.Select(entry => entry.Position).ToList());
        var conservative = ranking.Select(entry => new CommunityStrength(entry.Rating, entry.Deviation).Conservative).ToList();

        CollectionAssert.AreEqual(conservative.OrderByDescending(score => score).ToList(), conservative);
        Assert.IsTrue(ranking.All(entry => entry.Deviation > 0));
        Assert.IsTrue(ranking.All(entry => entry.Name == profiles[entry.CommunityId]));
        Assert.IsTrue(ranking.All(entry => entry.Comparisons == entry.Wins + entry.Draws + entry.Losses));
    }

    [TestMethod]
    public async Task TheVoterListCountsTheVotesOfEachVoter()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, VotesPerDay = EnoughVotes });
        await community.AnswerAsync(community.TeamA[1], CommunityVoteOutcome.Left);
        await community.AnswerAsync(community.Voter, CommunityVoteOutcome.Left);
        await community.AnswerAsync(community.Voter, CommunityVoteOutcome.UnknownBoth);

        var voters = await community.Hub.RunAsync(services => services.GetRequiredService<ICommunityRankingService>().VotersAsync(0, 10));

        Assert.HasCount(2, voters);
        Assert.AreEqual(community.Voter, voters[0].CommunityId);
        Assert.AreEqual(2, voters[0].Votes);
        Assert.AreEqual(1, voters[1].Votes);
        Assert.IsFalse(string.IsNullOrEmpty(voters[0].Name));
    }

    private static async Task<int> VoteAsync(CommunityTestHub community, IReadOnlyList<long> voters)
    {
        var votes = 0;
        var active = voters.ToList();

        while (active.Count > 0)
            foreach (var voter in active.ToList())
            {
                var outcome = Outcomes[votes % Outcomes.Length];

                if (await community.AnswerAsync(voter, outcome) == null)
                    active.Remove(voter);
                else
                    votes++;
            }

        return votes;
    }
}
