using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Helpers;

namespace L4D2PlayStats.Hub.Tests.Community;

[TestClass]
public class CommunityEloCalculatorTests
{
    private const int KFactor = 32;
    private const double Precision = 0.01;

    [TestMethod]
    public void AWinBetweenEqualsMovesHalfTheKFactor()
    {
        var change = CommunityEloCalculator.Calculate(1500, 1500, CommunityVoteOutcome.Left, KFactor);

        Assert.AreEqual(1516, change.Left, Precision);
        Assert.AreEqual(1484, change.Right, Precision);
    }

    [TestMethod]
    public void ALossBetweenEqualsMovesHalfTheKFactorTheOtherWay()
    {
        var change = CommunityEloCalculator.Calculate(1500, 1500, CommunityVoteOutcome.Right, KFactor);

        Assert.AreEqual(1484, change.Left, Precision);
        Assert.AreEqual(1516, change.Right, Precision);
    }

    [TestMethod]
    public void ADrawBetweenEqualsChangesNothing()
    {
        var change = CommunityEloCalculator.Calculate(1500, 1500, CommunityVoteOutcome.SameLevel, KFactor);

        Assert.AreEqual(1500, change.Left, Precision);
        Assert.AreEqual(1500, change.Right, Precision);
    }

    [TestMethod]
    public void ADrawPullsTheStrongerPlayerDownAndTheWeakerUp()
    {
        var change = CommunityEloCalculator.Calculate(1600, 1400, CommunityVoteOutcome.SameLevel, KFactor);

        Assert.AreEqual(1591.69, change.Left, Precision);
        Assert.AreEqual(1408.31, change.Right, Precision);
    }

    [TestMethod]
    public void TheFavouriteGainsLittleFromAnExpectedWin()
    {
        var change = CommunityEloCalculator.Calculate(1600, 1400, CommunityVoteOutcome.Left, KFactor);

        Assert.AreEqual(1607.69, change.Left, Precision);
        Assert.AreEqual(1392.31, change.Right, Precision);
    }

    [TestMethod]
    public void AnUpsetMovesTheRatingsTheMost()
    {
        var change = CommunityEloCalculator.Calculate(1600, 1400, CommunityVoteOutcome.Right, KFactor);

        Assert.AreEqual(1575.69, change.Left, Precision);
        Assert.AreEqual(1424.31, change.Right, Precision);
    }

    [TestMethod]
    public void WhatOnePlayerGainsTheOtherLoses()
    {
        var change = CommunityEloCalculator.Calculate(1712, 1345, CommunityVoteOutcome.Right, KFactor);

        Assert.AreEqual(1712 + 1345, change.Left + change.Right, Precision);
    }

    [TestMethod]
    public void TheExpectedScoresOfAPairAddUpToOne()
    {
        var left = CommunityEloCalculator.Expected(1630, 1480);
        var right = CommunityEloCalculator.Expected(1480, 1630);

        Assert.AreEqual(1, left + right, Precision);
        Assert.IsGreaterThan(right, left);
    }

    [TestMethod]
    [DataRow(CommunityVoteOutcome.UnknownLeft)]
    [DataRow(CommunityVoteOutcome.UnknownRight)]
    [DataRow(CommunityVoteOutcome.UnknownBoth)]
    public void NotKnowingAPlayerChangesNoRating(CommunityVoteOutcome outcome)
    {
        var change = CommunityEloCalculator.Calculate(1600, 1400, outcome, KFactor);

        Assert.AreEqual(1600, change.Left);
        Assert.AreEqual(1400, change.Right);
    }

    [TestMethod]
    public void ABiggerKFactorMovesTheRatingsMore()
    {
        var small = CommunityEloCalculator.Calculate(1500, 1500, CommunityVoteOutcome.Left, 10);
        var big = CommunityEloCalculator.Calculate(1500, 1500, CommunityVoteOutcome.Left, 40);

        Assert.AreEqual(1505, small.Left, Precision);
        Assert.AreEqual(1520, big.Left, Precision);
    }
}
