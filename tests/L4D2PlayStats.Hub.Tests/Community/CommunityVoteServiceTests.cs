using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Community;
using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Services;
using L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Community.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Community;

[TestClass]
public class CommunityVoteServiceTests
{
    private const int EnoughVotes = 1000;
    private const double Precision = 0.01;

    [TestMethod]
    public async Task APinnedRandomSourcePinsThePairOnOffer()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        var others = community.Everyone.Where(id => id != community.Voter).OrderBy(id => id).ToList();

        var straight = await community.Hub.RunAsync(services => Pinned(services, 0, 0, 0).BallotAsync(community.Voter));

        await community.VoteAsync(community.Voter, straight.Left!.CommunityId, straight.Right!.CommunityId, CommunityVoteOutcome.UnknownBoth);

        var swapped = await community.Hub.RunAsync(services => Pinned(services, 1, 2, 1).BallotAsync(community.Voter));

        Assert.AreEqual(others[0], straight.Left.CommunityId);
        Assert.AreEqual(others[1], straight.Right.CommunityId);
        Assert.AreEqual(others[5], swapped.Left!.CommunityId);
        Assert.AreEqual(others[3], swapped.Right!.CommunityId);
    }

    [TestMethod]
    public async Task ByDefaultTenMatchesAreNeededToVote()
    {
        using var community = new CommunityTestHub();

        for (var day = 0; day < 9; day++)
            await community.PlayAsync(CommunityTestHub.Night.AddDays(-day));

        var before = await community.BallotAsync(community.Voter);

        await community.PlayAsync(CommunityTestHub.Night.AddDays(-9));

        var after = await community.BallotAsync(community.Voter);

        Assert.AreEqual(CommunityBallotStatus.TooFewMatches, before.Status);
        Assert.AreEqual(9, before.Matches);
        Assert.AreEqual(10, before.MinimumMatches);
        Assert.IsNull(before.Left);
        Assert.AreEqual(CommunityBallotStatus.Ready, after.Status);
        Assert.AreEqual(10, after.Matches);
    }

    [TestMethod]
    public async Task MatchesOfEverySeasonCountForTheMinimum()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 2 });

        var before = await community.BallotAsync(community.Voter);

        await community.PlayAsync(CommunityTestHub.Night.AddMonths(-7));

        var after = await community.BallotAsync(community.Voter);
        var seasons = await community.Hub.RunAsync(services => services.GetRequiredService<HubDbContext>().Seasons.CountAsync());

        Assert.AreEqual(CommunityBallotStatus.TooFewMatches, before.Status);
        Assert.AreEqual(CommunityBallotStatus.Ready, after.Status);
        Assert.AreEqual(2, after.Matches);
        Assert.IsGreaterThan(1, seasons);
    }

    [TestMethod]
    public async Task AMatchThePlayerDidNotTakePartInDoesNotCount()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 2 });
        await community.PlayAsync(CommunityTestHub.Night.AddDays(-1), community.Voter);

        var ballot = await community.BallotAsync(community.Voter);

        Assert.AreEqual(CommunityBallotStatus.TooFewMatches, ballot.Status);
        Assert.AreEqual(1, ballot.Matches);
    }

    [TestMethod]
    public async Task AVoterWithTooFewMatchesCannotPostAVote()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 2 });

        var result = await community.VoteAsync(community.Voter, community.TeamA[1], community.TeamA[2], CommunityVoteOutcome.Left);

        Assert.AreEqual(CommunityVoteResult.TooFewMatches, result);
        Assert.IsEmpty(await community.VotesAsync());
    }

    [TestMethod]
    public async Task TheVoterJudgesEveryOtherPairOnceAndNeverThemselves()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, VotesPerDay = EnoughVotes });

        var pairs = await community.AnswerEverythingAsync(community.Voter, CommunityVoteOutcome.SameLevel);
        var ballot = await community.BallotAsync(community.Voter);

        Assert.HasCount(21, pairs);
        Assert.HasCount(21, pairs.Select(pair => pair.Normalized).Distinct().ToList());
        Assert.IsFalse(pairs.Any(pair => pair.Contains(community.Voter)));
        Assert.IsFalse(pairs.Any(pair => pair.Left == pair.Right));
        Assert.AreEqual(CommunityBallotStatus.NobodyLeft, ballot.Status);
    }

    [TestMethod]
    public async Task ThePairWaitsForItsAnswer()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        var first = await community.BallotAsync(community.Voter);

        for (var attempt = 0; attempt < 10; attempt++)
        {
            var again = await community.BallotAsync(community.Voter);

            Assert.AreEqual(first.Left, again.Left);
            Assert.AreEqual(first.Right, again.Right);
        }
    }

    [TestMethod]
    public async Task TheBallotShowsTheNamesOfThePlayers()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        var ballot = await community.BallotAsync(community.Voter);
        var profiles = await community.Hub.RunAsync(services => services.GetRequiredService<HubDbContext>().PlayerProfiles.AsNoTracking().ToDictionaryAsync(p => p.CommunityId, p => p.Name));

        Assert.AreEqual(profiles[ballot.Left!.CommunityId], ballot.Left.Name);
        Assert.AreEqual(profiles[ballot.Right!.CommunityId], ballot.Right.Name);
        Assert.IsFalse(string.IsNullOrEmpty(ballot.Left.Name));
    }

    [TestMethod]
    public async Task AVoteForAPairThatWasNotOfferedIsRefused()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        var ballot = await community.BallotAsync(community.Voter);
        var other = community.Everyone.First(id => id != community.Voter && id != ballot.Left!.CommunityId && id != ballot.Right!.CommunityId);

        var arbitrary = await community.VoteAsync(community.Voter, ballot.Left!.CommunityId, other, CommunityVoteOutcome.Left);
        var swapped = await community.VoteAsync(community.Voter, ballot.Right!.CommunityId, ballot.Left.CommunityId, CommunityVoteOutcome.Left);

        Assert.AreEqual(CommunityVoteResult.NotOffered, arbitrary);
        Assert.AreEqual(CommunityVoteResult.NotOffered, swapped);
        Assert.IsEmpty(await community.VotesAsync());
        Assert.IsEmpty(await community.RatingsAsync());
    }

    [TestMethod]
    public async Task AVoteBeforeAnyPairWasOfferedIsRefused()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        var result = await community.VoteAsync(community.Voter, community.TeamA[1], community.TeamA[2], CommunityVoteOutcome.Left);

        Assert.AreEqual(CommunityVoteResult.NotOffered, result);
        Assert.IsEmpty(await community.VotesAsync());
    }

    [TestMethod]
    public async Task NobodyVotesOnThemselves()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        var ballot = await community.BallotAsync(community.Voter);
        var direct = await community.VoteAsync(community.Voter, community.Voter, ballot.Right!.CommunityId, CommunityVoteOutcome.Left);

        await community.Hub.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();
            var offer = await dbContext.Set<CommunityOffer>().SingleAsync(o => o.VoterId == community.Voter);

            offer.LeftPlayerId = community.Voter;

            await dbContext.SaveChangesAsync();
        });

        var forged = await community.VoteAsync(community.Voter, community.Voter, ballot.Right.CommunityId, CommunityVoteOutcome.Left);

        Assert.AreEqual(CommunityVoteResult.NotOffered, direct);
        Assert.AreEqual(CommunityVoteResult.NotOffered, forged);
        Assert.IsEmpty(await community.VotesAsync());
        Assert.IsEmpty(await community.RatingsAsync());
    }

    [TestMethod]
    public async Task TheSameAnswerCannotBePostedTwice()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        var pair = (await community.AnswerAsync(community.Voter, CommunityVoteOutcome.Left))!;
        var repeated = await community.VoteAsync(community.Voter, pair.Left, pair.Right, CommunityVoteOutcome.Left);

        await community.BallotAsync(community.Voter);

        var afterNewOffer = await community.VoteAsync(community.Voter, pair.Left, pair.Right, CommunityVoteOutcome.Left);

        Assert.AreEqual(CommunityVoteResult.NotOffered, repeated);
        Assert.AreEqual(CommunityVoteResult.NotOffered, afterNewOffer);
        Assert.HasCount(1, await community.VotesAsync());
    }

    [TestMethod]
    public async Task AnAnswerThatIsNotOneOfTheChoicesIsRefused()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        var ballot = await community.BallotAsync(community.Voter);
        var result = await community.VoteAsync(community.Voter, ballot.Left!.CommunityId, ballot.Right!.CommunityId, 0);

        Assert.AreEqual(CommunityVoteResult.InvalidOutcome, result);
        Assert.IsEmpty(await community.VotesAsync());
    }

    [TestMethod]
    public async Task ChoosingAPlayerIsAWinForThemAndALossForTheOther()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        var chosenLeft = (await community.AnswerAsync(community.Voter, CommunityVoteOutcome.Left))!;
        var ratings = (await community.RatingsAsync()).ToDictionary(r => r.CommunityId);
        var winner = ratings[chosenLeft.Left];
        var loser = ratings[chosenLeft.Right];

        Assert.HasCount(2, ratings);
        Assert.AreEqual(1516, winner.Rating, Precision);
        Assert.AreEqual(1484, loser.Rating, Precision);
        Assert.AreEqual((1, 1, 0, 0), (winner.Comparisons, winner.Wins, winner.Losses, winner.Draws));
        Assert.AreEqual((1, 0, 1, 0), (loser.Comparisons, loser.Wins, loser.Losses, loser.Draws));
    }

    [TestMethod]
    public async Task ChoosingTheRightPlayerIsAWinForTheRightPlayer()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, KFactor = 20, InitialRating = 1000 });

        var pair = (await community.AnswerAsync(community.Voter, CommunityVoteOutcome.Right))!;
        var ratings = (await community.RatingsAsync()).ToDictionary(r => r.CommunityId);

        Assert.AreEqual(990, ratings[pair.Left].Rating, Precision);
        Assert.AreEqual(1010, ratings[pair.Right].Rating, Precision);
        Assert.AreEqual(1, ratings[pair.Left].Losses);
        Assert.AreEqual(1, ratings[pair.Right].Wins);
    }

    [TestMethod]
    public async Task TheSameLevelIsADraw()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        var first = (await community.AnswerAsync(community.Voter, CommunityVoteOutcome.Left))!;
        var other = community.Everyone.First(id => id != community.Voter && !first.Contains(id));

        await community.Hub.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();

            dbContext.Add(new CommunityOffer { VoterId = other, LeftPlayerId = first.Left, RightPlayerId = first.Right });

            await dbContext.SaveChangesAsync();
        });

        var result = await community.VoteAsync(other, first.Left, first.Right, CommunityVoteOutcome.SameLevel);
        var ratings = (await community.RatingsAsync()).ToDictionary(r => r.CommunityId);
        var stronger = ratings[first.Left];
        var weaker = ratings[first.Right];

        Assert.AreEqual(CommunityVoteResult.Accepted, result);
        Assert.AreEqual(1514.53, stronger.Rating, Precision);
        Assert.AreEqual(1485.47, weaker.Rating, Precision);
        Assert.AreEqual((2, 1, 0, 1), (stronger.Comparisons, stronger.Wins, stronger.Losses, stronger.Draws));
        Assert.AreEqual((2, 0, 1, 1), (weaker.Comparisons, weaker.Wins, weaker.Losses, weaker.Draws));
    }

    [TestMethod]
    [DataRow(CommunityVoteOutcome.UnknownLeft)]
    [DataRow(CommunityVoteOutcome.UnknownRight)]
    [DataRow(CommunityVoteOutcome.UnknownBoth)]
    [DataRow(CommunityVoteOutcome.UnknownOne)]
    public async Task NotKnowingAPlayerChangesNoRating(CommunityVoteOutcome outcome)
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });
        await community.AnswerAsync(community.Voter, CommunityVoteOutcome.Left);

        var before = CommunityTestHub.Snapshot(await community.RatingsAsync());

        await community.AnswerAsync(community.Voter, outcome);

        var after = CommunityTestHub.Snapshot(await community.RatingsAsync());
        var votes = await community.VotesAsync();

        Assert.AreEqual(before, after);
        Assert.HasCount(2, votes);
        Assert.AreEqual(outcome, votes[1].Outcome);
    }

    [TestMethod]
    public async Task APlayerTheVoterDoesNotKnowIsNotShownAgain()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, VotesPerDay = EnoughVotes });

        var unknown = (await community.AnswerAsync(community.Voter, CommunityVoteOutcome.UnknownLeft))!;
        var pairs = await community.AnswerEverythingAsync(community.Voter, CommunityVoteOutcome.SameLevel);

        Assert.HasCount(15, pairs);
        Assert.IsFalse(pairs.Any(pair => pair.Contains(unknown.Left)));
        Assert.IsTrue(pairs.Any(pair => pair.Contains(unknown.Right)));
    }

    [TestMethod]
    public async Task NotKnowingTheRightPlayerHidesOnlyTheRightPlayer()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, VotesPerDay = EnoughVotes });

        var unknown = (await community.AnswerAsync(community.Voter, CommunityVoteOutcome.UnknownRight))!;
        var pairs = await community.AnswerEverythingAsync(community.Voter, CommunityVoteOutcome.SameLevel);

        Assert.HasCount(15, pairs);
        Assert.IsFalse(pairs.Any(pair => pair.Contains(unknown.Right)));
        Assert.IsTrue(pairs.Any(pair => pair.Contains(unknown.Left)));
    }

    [TestMethod]
    public async Task NotKnowingEitherPlayerHidesBoth()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, VotesPerDay = EnoughVotes });

        var unknown = (await community.AnswerAsync(community.Voter, CommunityVoteOutcome.UnknownBoth))!;
        var pairs = await community.AnswerEverythingAsync(community.Voter, CommunityVoteOutcome.SameLevel);

        Assert.HasCount(10, pairs);
        Assert.IsFalse(pairs.Any(pair => pair.Contains(unknown.Left) || pair.Contains(unknown.Right)));
    }

    [TestMethod]
    public async Task NotKnowingOneOfThemSkipsOnlyThatPair()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, VotesPerDay = EnoughVotes });

        var skipped = (await community.AnswerAsync(community.Voter, CommunityVoteOutcome.UnknownOne))!;
        var pairs = await community.AnswerEverythingAsync(community.Voter, CommunityVoteOutcome.SameLevel);

        Assert.HasCount(20, pairs);
        Assert.IsFalse(pairs.Any(pair => pair.Contains(skipped.Left) && pair.Contains(skipped.Right)));
        Assert.IsTrue(pairs.Any(pair => pair.Contains(skipped.Left)));
        Assert.IsTrue(pairs.Any(pair => pair.Contains(skipped.Right)));
    }

    [TestMethod]
    public async Task APlayerUnknownToOneVoterIsStillShownToAnother()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, VotesPerDay = EnoughVotes });

        var unknown = (await community.AnswerAsync(community.Voter, CommunityVoteOutcome.UnknownBoth))!;
        var other = community.Everyone.First(id => id != community.Voter && !unknown.Contains(id));
        var pairs = await community.AnswerEverythingAsync(other, CommunityVoteOutcome.SameLevel);

        Assert.HasCount(21, pairs);
        Assert.IsTrue(pairs.Any(pair => pair.Contains(unknown.Left)));
        Assert.IsTrue(pairs.Any(pair => pair.Contains(unknown.Right)));
    }

    [TestMethod]
    public async Task TheVotesOfTheDayRunOut()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, VotesPerDay = 3 });

        var first = await community.BallotAsync(community.Voter);

        await community.AnswerAsync(community.Voter, CommunityVoteOutcome.Left);
        await community.AnswerAsync(community.Voter, CommunityVoteOutcome.Right);

        var third = await community.BallotAsync(community.Voter);

        await community.AnswerAsync(community.Voter, CommunityVoteOutcome.SameLevel);

        var last = await community.BallotAsync(community.Voter);
        var refused = await community.VoteAsync(community.Voter, third.Left!.CommunityId, third.Right!.CommunityId, CommunityVoteOutcome.Left);

        Assert.AreEqual(3, first.VotesLeft);
        Assert.AreEqual(3, first.VotesPerDay);
        Assert.AreEqual(1, third.VotesLeft);
        Assert.AreEqual(CommunityBallotStatus.LimitReached, last.Status);
        Assert.AreEqual(0, last.VotesLeft);
        Assert.IsNull(last.Left);
        Assert.AreEqual(CommunityVoteResult.LimitReached, refused);
        Assert.HasCount(3, await community.VotesAsync());
    }

    [TestMethod]
    public async Task TheLimitOfOneVoterDoesNotStopAnother()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, VotesPerDay = 1 });
        await community.AnswerAsync(community.Voter, CommunityVoteOutcome.Left);

        var mine = await community.BallotAsync(community.Voter);
        var theirs = await community.BallotAsync(community.TeamB[0]);

        Assert.AreEqual(CommunityBallotStatus.LimitReached, mine.Status);
        Assert.AreEqual(CommunityBallotStatus.Ready, theirs.Status);
    }

    [TestMethod]
    public async Task NotKnowingAPlayerDoesNotUseUpAVoteOfTheDay()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, VotesPerDay = 2 });
        await community.AnswerAsync(community.Voter, CommunityVoteOutcome.UnknownLeft);
        await community.AnswerAsync(community.Voter, CommunityVoteOutcome.UnknownBoth);

        var ballot = await community.BallotAsync(community.Voter);

        Assert.AreEqual(CommunityBallotStatus.Ready, ballot.Status);
        Assert.AreEqual(2, ballot.VotesLeft);
    }

    [TestMethod]
    public async Task TheVotesComeBackOnTheNextDayOfTheSite()
    {
        using var community = new CommunityTestHub();

        community.Hub.Clock.UtcNow = new DateTime(2026, 11, 15, 23, 30, 0, DateTimeKind.Utc);

        await community.SaveAsync(new SiteSettings { TimeZone = "America/Sao_Paulo" });
        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, VotesPerDay = 2 });
        await community.AnswerAsync(community.Voter, CommunityVoteOutcome.Left);
        await community.AnswerAsync(community.Voter, CommunityVoteOutcome.Left);

        var sameEvening = await community.BallotAsync(community.Voter);

        community.Hub.Clock.Advance(TimeSpan.FromHours(1));

        var afterMidnightUtc = await community.BallotAsync(community.Voter);

        community.Hub.Clock.Advance(TimeSpan.FromHours(3));

        var nextSiteDay = await community.BallotAsync(community.Voter);

        await community.AnswerAsync(community.Voter, CommunityVoteOutcome.Left);

        var votes = await community.VotesAsync();

        Assert.AreEqual(CommunityBallotStatus.LimitReached, sameEvening.Status);
        Assert.AreEqual(CommunityBallotStatus.LimitReached, afterMidnightUtc.Status);
        Assert.AreEqual(CommunityBallotStatus.Ready, nextSiteDay.Status);
        Assert.AreEqual(2, nextSiteDay.VotesLeft);
        Assert.AreEqual(new DateTime(2026, 11, 15), votes[0].SiteDay);
        Assert.AreEqual(new DateTime(2026, 11, 16), votes[2].SiteDay);
    }

    [TestMethod]
    public async Task PlayersTheVoterPlayedWithAreOfferedBeforeStrangers()
    {
        using var community = new CommunityTestHub();

        var strangers = community.TeamB.Take(3).ToArray();
        var known = community.Everyone.Where(id => id != community.Voter && !strangers.Contains(id)).ToList();

        await community.PlayAsync(CommunityTestHub.Night, strangers);
        await community.PlayAsync(CommunityTestHub.Night.AddDays(-1), community.Voter);
        await community.SaveAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, VotesPerDay = EnoughVotes });

        var pairs = await community.AnswerEverythingAsync(community.Voter, CommunityVoteOutcome.SameLevel);
        var first = pairs.Take(6).ToList();
        var rest = pairs.Skip(6).ToList();

        Assert.HasCount(4, known);
        Assert.HasCount(21, pairs);
        Assert.IsTrue(first.All(pair => known.Contains(pair.Left) && known.Contains(pair.Right)));
        Assert.IsTrue(rest.All(pair => strangers.Contains(pair.Left) || strangers.Contains(pair.Right)));
    }

    [TestMethod]
    public async Task PlayersWithTooFewMatchesAreNotCandidates()
    {
        using var community = new CommunityTestHub();

        var newcomers = community.TeamB.Take(2).ToArray();

        await community.PlayAsync(CommunityTestHub.Night);
        await community.PlayAsync(CommunityTestHub.Night.AddDays(-1), newcomers);
        await community.SaveAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, MinimumMatchesToBeCandidate = 2, VotesPerDay = EnoughVotes });

        var pairs = await community.AnswerEverythingAsync(community.Voter, CommunityVoteOutcome.SameLevel);

        Assert.HasCount(10, pairs);
        Assert.IsFalse(pairs.Any(pair => newcomers.Contains(pair.Left) || newcomers.Contains(pair.Right)));
    }

    [TestMethod]
    public async Task PlayersWhoHaveNotPlayedForTooLongAreNotCandidates()
    {
        using var community = new CommunityTestHub();

        var veterans = community.TeamB.Take(2).ToArray();

        await community.PlayAsync(CommunityTestHub.Night.AddDays(-200));
        await community.PlayAsync(CommunityTestHub.Night, veterans);
        await community.SaveAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, CandidatePlayedWithinDays = 90, VotesPerDay = EnoughVotes });

        var pairs = await community.AnswerEverythingAsync(community.Voter, CommunityVoteOutcome.SameLevel);

        Assert.HasCount(10, pairs);
        Assert.IsFalse(pairs.Any(pair => veterans.Contains(pair.Left) || veterans.Contains(pair.Right)));
    }

    [TestMethod]
    public async Task WithZeroDaysPlayersAreCandidatesHoweverLongAgoTheyPlayed()
    {
        using var community = new CommunityTestHub();

        var veterans = community.TeamB.Take(2).ToArray();

        await community.PlayAsync(CommunityTestHub.Night.AddDays(-200));
        await community.PlayAsync(CommunityTestHub.Night, veterans);
        await community.SaveAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, CandidatePlayedWithinDays = 0, VotesPerDay = EnoughVotes });

        var pairs = await community.AnswerEverythingAsync(community.Voter, CommunityVoteOutcome.SameLevel);

        Assert.HasCount(21, pairs);
        Assert.IsTrue(pairs.Any(pair => veterans.Contains(pair.Left) || veterans.Contains(pair.Right)));
    }

    [TestMethod]
    public async Task TheNamesOfAPlayerComeFromTheMostUsedToTheLeastUsed()
    {
        using var community = new CommunityTestHub();

        var player = community.TeamB[0];
        string[] names = ["Gamma", "Alpha", "Beta", "alpha ", "Alpha", "Beta"];

        for (var day = 0; day < names.Length; day++)
            await community.PlayAsync(CommunityTestHub.Night.AddDays(-day));

        await community.Hub.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();
            var played = await dbContext.MatchPlayers.Where(p => p.CommunityId == player).OrderBy(p => p.MatchId).ToListAsync();

            for (var index = 0; index < played.Count; index++)
                played[index].Name = names[index];

            await dbContext.SaveChangesAsync();
        });

        var used = await community.Hub.RunAsync(services => services.GetRequiredService<ICommunityPlayerService>().MostUsedNamesAsync([player], 2));
        var all = await community.Hub.RunAsync(services => services.GetRequiredService<ICommunityPlayerService>().MostUsedNamesAsync([player], 10));

        CollectionAssert.AreEqual(new List<string> { "Alpha", "Beta" }, used[player]);
        CollectionAssert.AreEqual(new List<string> { "Alpha", "Beta", "Gamma" }, all[player]);
    }

    [TestMethod]
    public void TheDaysSinceTheLastMatchGiveTheOldestMatchThatCounts()
    {
        var now = new DateTime(2026, 11, 15, 12, 0, 0, DateTimeKind.Utc);
        var settings = new CommunityRankingSettings { CandidatePlayedWithinDays = -5 };

        settings.Normalize();

        Assert.AreEqual(0, settings.CandidatePlayedWithinDays);
        Assert.IsNull(settings.CandidatesPlayedSince(now));
        Assert.AreEqual(now.AddDays(-90), new CommunityRankingSettings { CandidatePlayedWithinDays = 90 }.CandidatesPlayedSince(now));
    }

    [TestMethod]
    public async Task APlayerWhoStopsBeingACandidateLeavesThePairOnOffer()
    {
        using var community = new CommunityTestHub();

        await community.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        var ballot = await community.BallotAsync(community.Voter);
        var left = ballot.Left!.CommunityId;

        await community.PlayAsync(CommunityTestHub.Night.AddDays(-1), left);
        await community.SaveAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, MinimumMatchesToBeCandidate = 2 });

        var refused = await community.VoteAsync(community.Voter, left, ballot.Right!.CommunityId, CommunityVoteOutcome.Left);
        var next = await community.BallotAsync(community.Voter);

        Assert.AreEqual(CommunityVoteResult.NotOffered, refused);
        Assert.AreEqual(CommunityBallotStatus.Ready, next.Status);
        Assert.AreNotEqual(left, next.Left!.CommunityId);
        Assert.AreNotEqual(left, next.Right!.CommunityId);
        Assert.IsEmpty(await community.VotesAsync());
    }

    private static CommunityVoteService Pinned(IServiceProvider services, params int[] values)
    {
        return ActivatorUtilities.CreateInstance<CommunityVoteService>(services, new SequenceRandomService(values));
    }
}