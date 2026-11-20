namespace L4D2PlayStats.Hub.Core.Modules.Community.Models;

public record CommunityBallotPlayer(long CommunityId, string Name, IReadOnlyList<string> OtherNames)
{
    public virtual bool Equals(CommunityBallotPlayer? other)
    {
        return other != null && CommunityId == other.CommunityId && Name == other.Name && OtherNames.SequenceEqual(other.OtherNames);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CommunityId, Name);
    }
}