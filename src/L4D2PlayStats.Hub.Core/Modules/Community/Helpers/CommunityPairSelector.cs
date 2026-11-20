using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Helpers;

public static class CommunityPairSelector
{
    public static CommunityPair? Select(IReadOnlyList<CommunityCandidate> candidates, IReadOnlySet<CommunityPair> judged, ICommunityRandomService random)
    {
        var familiar = candidates.Where(candidate => candidate.Familiar).ToList();

        return SelectFrom(familiar, judged, random) ?? SelectFrom(candidates, judged, random);
    }

    private static CommunityPair? SelectFrom(IReadOnlyList<CommunityCandidate> pool, IReadOnlySet<CommunityPair> judged, ICommunityRandomService random)
    {
        var anchors = pool
            .OrderByDescending(candidate => candidate.Familiar)
            .ThenBy(candidate => candidate.Comparisons)
            .ThenBy(candidate => candidate.CommunityId)
            .Where(candidate => Opponents(candidate, pool, judged).Any())
            .Take(AppConsts.CommunityAnchorPoolSize)
            .ToList();

        if (anchors.Count == 0)
            return null;

        var anchor = anchors[random.Next(anchors.Count)];

        var opponents = Opponents(anchor, pool, judged)
            .OrderByDescending(candidate => candidate.Familiar)
            .ThenBy(candidate => Math.Abs(candidate.Rating - anchor.Rating))
            .ThenBy(candidate => candidate.Comparisons)
            .ThenBy(candidate => candidate.CommunityId)
            .Take(AppConsts.CommunityOpponentPoolSize)
            .ToList();

        var opponent = opponents[random.Next(opponents.Count)];

        return random.Next(AppConsts.CommunityPairSides) == 0
            ? new CommunityPair(anchor.CommunityId, opponent.CommunityId)
            : new CommunityPair(opponent.CommunityId, anchor.CommunityId);
    }

    private static IEnumerable<CommunityCandidate> Opponents(CommunityCandidate anchor, IReadOnlyList<CommunityCandidate> pool, IReadOnlySet<CommunityPair> judged)
    {
        return pool.Where(candidate => candidate.CommunityId != anchor.CommunityId && !judged.Contains(new CommunityPair(anchor.CommunityId, candidate.CommunityId).Normalized));
    }
}
