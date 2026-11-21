using L4D2PlayStats.Hub.Core;
using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;

namespace L4D2PlayStats.Hub.Tests.Community;

[TestClass]
public class CommunityRatingFitTests
{
    private const double Center = 1500;
    private const double Precision = 0.01;

    [TestMethod]
    public void EqualRatingsAreExpectedToWinHalfTheTime()
    {
        Assert.AreEqual(0.5, CommunityRatingFit.Expected(1500, 1500), Precision);
        Assert.AreEqual(0.909, CommunityRatingFit.Expected(1900, 1500), 0.001);
    }

    [TestMethod]
    [DataRow(CommunityVoteOutcome.Left, 1d)]
    [DataRow(CommunityVoteOutcome.Right, 0d)]
    [DataRow(CommunityVoteOutcome.SameLevel, 0.5d)]
    public void EachAnswerHasItsScore(CommunityVoteOutcome outcome, double score)
    {
        Assert.AreEqual(score, CommunityRatingFit.Score(outcome));
    }

    [TestMethod]
    public void NoVotesGiveNoRatings()
    {
        Assert.HasCount(0, CommunityRatingFit.Fit([], Center));
    }

    [TestMethod]
    public void OneWinSplitsTheTwoPlayersAroundTheCenter()
    {
        var ratings = CommunityRatingFit.Fit([new CommunityComparison(1, 2, 1)], Center);

        Assert.IsGreaterThan(Center, ratings[1].Rating);
        Assert.AreEqual(2 * Center, ratings[1].Rating + ratings[2].Rating, Precision);
        Assert.AreEqual(ratings[1].Deviation, ratings[2].Deviation, Precision);
    }

    [TestMethod]
    public void ASameLevelAnswerKeepsBothPlayersAtTheCenter()
    {
        var ratings = CommunityRatingFit.Fit([new CommunityComparison(1, 2, 0.5)], Center);

        Assert.AreEqual(Center, ratings[1].Rating, Precision);
        Assert.AreEqual(Center, ratings[2].Rating, Precision);
    }

    [TestMethod]
    public void TheOrderOfTheVotesDoesNotMatter()
    {
        List<CommunityComparison> votes =
        [
            new(1, 2, 1), new(2, 3, 1), new(3, 1, 0.5), new(1, 4, 0), new(4, 2, 1), new(3, 4, 1), new(2, 1, 1)
        ];

        var forward = CommunityRatingFit.Fit(votes, Center);
        var backward = CommunityRatingFit.Fit([.. Enumerable.Reverse(votes)], Center);

        foreach (var (player, strength) in forward)
        {
            Assert.AreEqual(strength.Rating, backward[player].Rating, Precision);
            Assert.AreEqual(strength.Deviation, backward[player].Deviation, Precision);
        }
    }

    [TestMethod]
    public void BeatingAStrongPlayerIsWorthMoreThanBeatingAWeakOne()
    {
        List<CommunityComparison> votes = [new(10, 1, 1), new(20, 9, 1)];

        for (var round = 0; round < 6; round++)
        {
            votes.Add(new CommunityComparison(1, 2, 1));
            votes.Add(new CommunityComparison(1, 3, 1));
            votes.Add(new CommunityComparison(4, 9, 1));
            votes.Add(new CommunityComparison(5, 9, 1));
        }

        var ratings = CommunityRatingFit.Fit(votes, Center);

        Assert.IsGreaterThan(ratings[20].Rating, ratings[10].Rating);
    }

    [TestMethod]
    public void MoreComparisonsMakeTheRatingMoreCertain()
    {
        List<CommunityComparison> votes = [new(1, 2, 1)];

        for (var round = 0; round < 10; round++)
        {
            votes.Add(new CommunityComparison(3, 4, 1));
            votes.Add(new CommunityComparison(4, 3, 1));
        }

        var ratings = CommunityRatingFit.Fit(votes, Center);

        Assert.IsLessThan(ratings[1].Deviation, ratings[3].Deviation);
        Assert.IsLessThan(AppConsts.CommunityPriorDeviation, ratings[1].Deviation);
    }

    [TestMethod]
    public void AFewLuckyVotesDoNotPutAPlayerAboveAProvenOne()
    {
        List<CommunityComparison> votes = [new(1, 2, 1), new(1, 3, 1)];

        for (var round = 0; round < 8; round++)
        {
            votes.Add(new CommunityComparison(10, 11, 1));
            votes.Add(new CommunityComparison(10, 12, 1));
            votes.Add(new CommunityComparison(10, 13, round == 0 ? 0 : 1));
        }

        var ratings = CommunityRatingFit.Fit(votes, Center);

        Assert.IsGreaterThan(ratings[1].Conservative, ratings[10].Conservative);
    }

    [TestMethod]
    public void APlayerWhoAlwaysWinsStaysWithinTheScale()
    {
        List<CommunityComparison> votes = [];

        for (var round = 0; round < 50; round++)
            votes.Add(new CommunityComparison(1, 2, 1));

        var ratings = CommunityRatingFit.Fit(votes, Center);

        Assert.IsLessThan(Center + 4 * AppConsts.CommunityPriorDeviation, ratings[1].Rating);
        Assert.IsGreaterThan(Center, ratings[1].Rating);
    }
}
