namespace L4D2PlayStats.Hub.Core.Modules.Championships;

public class ChampionshipTeam
{
    public int Id { get; set; }
    public int ChampionshipId { get; set; }
    public string Name { get; set; } = null!;
    public string? Tag { get; set; }
    public string? Flag { get; set; }
    public int Seed { get; set; }
    public int? DraftPosition { get; set; }
    public long? CaptainCommunityId { get; set; }
    public long? ViceCaptainCommunityId { get; set; }
    public string? PendingName { get; set; }
    public string? PendingTag { get; set; }
    public string? PendingFlag { get; set; }
    public DateTime? PendingAtUtc { get; set; }
    public long? PendingByCommunityId { get; set; }
    public string? RejectionReason { get; set; }
    public List<ChampionshipTeamMember> Members { get; set; } = [];

    public bool HasPendingIdentity => PendingName != null || PendingFlag != null;

    public HashSet<long> Players()
    {
        return Members.Select(member => member.CommunityId).ToHashSet();
    }

    public bool IsLeader(long communityId)
    {
        return CaptainCommunityId == communityId || ViceCaptainCommunityId == communityId;
    }
}
