namespace L4D2PlayStats.Hub.Core.Modules.Championships;

public class ChampionshipTeam
{
    public int Id { get; set; }
    public int ChampionshipId { get; set; }
    public string Name { get; set; } = null!;
    public string? Tag { get; set; }
    public int Seed { get; set; }
    public int? DraftPosition { get; set; }
    public long? CaptainCommunityId { get; set; }
    public long? ViceCaptainCommunityId { get; set; }
    public List<ChampionshipTeamMember> Members { get; set; } = [];

    public bool IsLeader(long communityId)
    {
        return CaptainCommunityId == communityId || ViceCaptainCommunityId == communityId;
    }
}
