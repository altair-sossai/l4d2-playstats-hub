using L4D2PlayStats.Hub.Core.Modules.Bans.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Bans;

public class BanAlert
{
    public int Id { get; set; }
    public int BanId { get; set; }
    public long CommunityId { get; set; }
    public BanAlertStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public long? ResolvedByCommunityId { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
}
