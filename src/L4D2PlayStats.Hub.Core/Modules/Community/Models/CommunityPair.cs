namespace L4D2PlayStats.Hub.Core.Modules.Community.Models;

public record CommunityPair(long Left, long Right)
{
    public CommunityPair Normalized => Left <= Right ? this : new CommunityPair(Right, Left);

    public bool Contains(long communityId)
    {
        return Left == communityId || Right == communityId;
    }
}
