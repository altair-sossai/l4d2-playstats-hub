using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Championships;

public class ChampionshipJoinRequest
{
    public int Id { get; set; }
    public int ChampionshipId { get; set; }
    public long CommunityId { get; set; }
    public string? Message { get; set; }
    public ChampionshipJoinRequestStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? AnsweredAtUtc { get; set; }
    public long? AnsweredBy { get; set; }
}
