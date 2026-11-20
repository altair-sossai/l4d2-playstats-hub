namespace L4D2PlayStats.Hub.Core.Modules.Community.Models;

public record CommunitySelection(IReadOnlyList<CommunityCandidate> Candidates, IReadOnlySet<CommunityPair> Judged)
{
    public bool Allows(CommunityPair pair)
    {
        return pair.Left != pair.Right
               && Candidates.Any(candidate => candidate.CommunityId == pair.Left)
               && Candidates.Any(candidate => candidate.CommunityId == pair.Right)
               && !Judged.Contains(pair.Normalized);
    }
}