using L4D2PlayStats.Hub.Core;
using L4D2PlayStats.Hub.Core.Modules.Community.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Tests.Community.Support;

namespace L4D2PlayStats.Hub.Tests.Community;

[TestClass]
public class CommunityMixBalancerTests
{
    [TestMethod]
    public void EightPlayersCanBeSplitInThirtyFiveWays()
    {
        Assert.HasCount(35, CommunityMixBalancer.Options(Players(1500, 1500, 1500, 1500, 1500, 1500, 1500, 1500)));
    }

    [TestMethod]
    public void EveryPlayerEndsInExactlyOneOfTwoTeamsOfTheSameSize()
    {
        var players = Players(2100, 1900, 1800, 1700, 1600, 1500, 1400, 1300);

        var split = CommunityMixBalancer.Split(players, new SequenceRandomService());

        Assert.HasCount(4, split.Survivors);
        Assert.HasCount(4, split.Infected);
        CollectionAssert.AreEquivalent(players.Select(player => player.CommunityId).ToList(), split.Survivors.Concat(split.Infected).ToList());
    }

    [TestMethod]
    public void TheTeamsAreBalanced()
    {
        var players = Players(2200, 2000, 1850, 1700, 1600, 1450, 1300, 1150);

        var split = CommunityMixBalancer.Split(players, new SequenceRandomService());

        Assert.IsGreaterThan(0.45, split.SurvivorsChance);
        Assert.IsLessThan(0.55, split.SurvivorsChance);
    }

    [TestMethod]
    public void TheTwoBestPlayersAreNotPutOnTheSameTeam()
    {
        var players = Players(2200, 2150, 1500, 1500, 1500, 1500, 1500, 1500);

        foreach (var value in Enumerable.Range(0, 10))
        {
            var split = CommunityMixBalancer.Split(players, new SequenceRandomService(value, value));

            Assert.AreEqual(1, split.Survivors.Count(communityId => communityId <= 2));
        }
    }

    [TestMethod]
    public void TheSameRandomSourceGivesTheSameTeams()
    {
        var players = Players(2100, 1900, 1800, 1700, 1600, 1500, 1400, 1300);

        var first = CommunityMixBalancer.Split(players, new SequenceRandomService(3, 1));
        var second = CommunityMixBalancer.Split(players, new SequenceRandomService(3, 1));

        CollectionAssert.AreEqual(first.Survivors.ToList(), second.Survivors.ToList());
        CollectionAssert.AreEqual(first.Infected.ToList(), second.Infected.ToList());
    }

    [TestMethod]
    public void EqualPlayersCanBeSplitInManyWays()
    {
        var players = Players(1500, 1500, 1500, 1500, 1500, 1500, 1500, 1500);

        var splits = Enumerable.Range(0, 35)
            .Select(value => CommunityMixBalancer.Split(players, new SequenceRandomService(value, 0)))
            .Select(split => string.Join(",", split.Survivors.Order()))
            .Distinct()
            .ToList();

        Assert.IsGreaterThan(10, splits.Count);
    }

    [TestMethod]
    public void TheRandomSourceDecidesWhichTeamStartsAsSurvivors()
    {
        var players = Players(2100, 1900, 1800, 1700, 1600, 1500, 1400, 1300);

        var first = CommunityMixBalancer.Split(players, new SequenceRandomService(0, 0));
        var second = CommunityMixBalancer.Split(players, new SequenceRandomService(0, 1));

        CollectionAssert.AreEqual(first.Survivors.ToList(), second.Infected.ToList());
        Assert.AreEqual(1, first.SurvivorsChance + second.SurvivorsChance, 0.0001);
    }

    private static List<CommunityMixPlayer> Players(params double[] ratings)
    {
        return [.. ratings.Select((rating, index) => new CommunityMixPlayer(index + 1, rating, AppConsts.CommunityPriorDeviation / 4))];
    }
}
