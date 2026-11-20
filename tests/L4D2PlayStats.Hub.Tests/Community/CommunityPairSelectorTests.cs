using L4D2PlayStats.Hub.Tests.Community.Support;
using L4D2PlayStats.Hub.Core.Modules.Community.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using System.Collections.Immutable;

namespace L4D2PlayStats.Hub.Tests.Community;

[TestClass]
public class CommunityPairSelectorTests
{
    private static readonly ImmutableHashSet<CommunityPair> Nothing = [];

    [TestMethod]
    public void NothingIsOfferedWithFewerThanTwoCandidates()
    {
        var nobody = CommunityPairSelector.Select([], Nothing, new SequenceRandomService());
        var alone = CommunityPairSelector.Select([Candidate(1)], Nothing, new SequenceRandomService());

        Assert.IsNull(nobody);
        Assert.IsNull(alone);
    }

    [TestMethod]
    public void APlayerIsNeverPairedWithThemselves()
    {
        var candidates = new[] { Candidate(1), Candidate(2), Candidate(3) };

        for (var seed = 0; seed < 30; seed++)
        {
            var pair = CommunityPairSelector.Select(candidates, Nothing, new SequenceRandomService(seed, seed + 1, seed + 2))!;

            Assert.AreNotEqual(pair.Left, pair.Right);
        }
    }

    [TestMethod]
    public void PlayersTheVoterPlayedWithComeBeforeStrangers()
    {
        var candidates = new[] { Candidate(1), Candidate(2), Candidate(3, familiar: true), Candidate(4), Candidate(5, familiar: true) };

        for (var seed = 0; seed < 30; seed++)
        {
            var pair = CommunityPairSelector.Select(candidates, Nothing, new SequenceRandomService(seed, seed + 1, seed + 2))!;

            Assert.AreEqual(new CommunityPair(3, 5), pair.Normalized);
        }
    }

    [TestMethod]
    public void StrangersAreOfferedOnceTheKnownPairsRunOut()
    {
        var candidates = new[] { Candidate(1), Candidate(2), Candidate(3, familiar: true), Candidate(4, familiar: true) };
        var judged = new HashSet<CommunityPair> { new(3, 4) };

        var pair = CommunityPairSelector.Select(candidates, judged, new SequenceRandomService())!;

        Assert.AreEqual(3, pair.Left);
        Assert.AreEqual(1, pair.Right);
    }

    [TestMethod]
    public void ThePlayerWithTheFewestComparisonsIsPickedFirst()
    {
        var candidates = new[] { Candidate(1, comparisons: 9), Candidate(2, comparisons: 4), Candidate(3, comparisons: 1), Candidate(4, comparisons: 6) };

        var pair = CommunityPairSelector.Select(candidates, Nothing, new SequenceRandomService())!;

        Assert.AreEqual(3, pair.Left);
    }

    [TestMethod]
    public void TheOpponentIsThePlayerWithTheClosestRating()
    {
        var candidates = new[] { Candidate(1), Candidate(2, 1900, 5), Candidate(3, 1540, 5), Candidate(4, 1380, 5) };

        var pair = CommunityPairSelector.Select(candidates, Nothing, new SequenceRandomService())!;

        Assert.AreEqual(1, pair.Left);
        Assert.AreEqual(3, pair.Right);
    }

    [TestMethod]
    public void OpponentsFarAwayInRatingStayOutWhileCloserOnesExist()
    {
        var candidates = new List<CommunityCandidate> { Candidate(1) };

        for (var id = 2; id <= 7; id++)
            candidates.Add(Candidate(id, 1500 + id * 10, 5));

        candidates.Add(Candidate(8, 2400, 5));

        for (var seed = 0; seed < 30; seed++)
        {
            var pair = CommunityPairSelector.Select(candidates, Nothing, new SequenceRandomService(0, seed, 0))!;

            Assert.AreEqual(1, pair.Left);
            Assert.AreNotEqual(8, pair.Right);
            Assert.AreNotEqual(7, pair.Right);
        }
    }

    [TestMethod]
    public void APairAlreadyJudgedIsNotOfferedAgainInEitherOrder()
    {
        var candidates = new[] { Candidate(1), Candidate(2), Candidate(3) };
        var judged = new HashSet<CommunityPair> { new CommunityPair(2, 1).Normalized, new CommunityPair(3, 1).Normalized };

        for (var seed = 0; seed < 30; seed++)
        {
            var pair = CommunityPairSelector.Select(candidates, judged, new SequenceRandomService(seed, seed + 1, seed + 2))!;

            Assert.AreEqual(new CommunityPair(2, 3), pair.Normalized);
        }
    }

    [TestMethod]
    public void NothingIsOfferedWhenEveryPairWasJudged()
    {
        var candidates = new[] { Candidate(1), Candidate(2), Candidate(3) };
        var judged = new HashSet<CommunityPair> { new(1, 2), new(1, 3), new(2, 3) };

        var pair = CommunityPairSelector.Select(candidates, judged, new SequenceRandomService());

        Assert.IsNull(pair);
    }

    [TestMethod]
    public void TheSameRandomSourceGivesTheSamePair()
    {
        var candidates = Enumerable.Range(1, 20).Select(id => Candidate(id, 1400 + id * 13, id % 4, id % 3 == 0)).ToList();

        for (var seed = 0; seed < 30; seed++)
        {
            var first = CommunityPairSelector.Select(candidates, Nothing, new SequenceRandomService(seed, seed * 7, seed * 3));
            var second = CommunityPairSelector.Select(candidates, Nothing, new SequenceRandomService(seed, seed * 7, seed * 3));

            Assert.AreEqual(first, second);
        }
    }

    [TestMethod]
    public void TheRandomSourceDecidesWhoGoesOnEachSide()
    {
        var candidates = new[] { Candidate(1), Candidate(2, comparisons: 3) };

        var straight = CommunityPairSelector.Select(candidates, Nothing, new SequenceRandomService(0, 0, 0))!;
        var swapped = CommunityPairSelector.Select(candidates, Nothing, new SequenceRandomService(0, 0, 1))!;

        Assert.AreEqual(new CommunityPair(1, 2), straight);
        Assert.AreEqual(new CommunityPair(2, 1), swapped);
    }

    private static CommunityCandidate Candidate(long communityId, double rating = 1500, int comparisons = 0, bool familiar = false)
    {
        return new CommunityCandidate(communityId, rating, comparisons, familiar);
    }
}
