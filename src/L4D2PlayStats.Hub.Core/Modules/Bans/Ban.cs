using L4D2PlayStats.Hub.Core.Modules.Bans.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Bans;

public class Ban
{
    public int Id { get; set; }
    public long CommunityId { get; set; }
    public string? Name { get; set; }
    public string? Reason { get; set; }
    public BanSource Source { get; set; }
    public string? Command { get; set; }
    public string? GameServerName { get; set; }
    public int? RelatedToBanId { get; set; }
    public long? CreatedByCommunityId { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? EndsAtUtc { get; set; }
    public long? UpdatedByCommunityId { get; set; }
    public string? UpdatedByName { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public long? LiftedByCommunityId { get; set; }
    public string? LiftedByName { get; set; }
    public DateTime? LiftedAtUtc { get; set; }
    public string? LiftReason { get; set; }
    public int Attempts { get; set; }
    public DateTime? LastAttemptAtUtc { get; set; }
}
