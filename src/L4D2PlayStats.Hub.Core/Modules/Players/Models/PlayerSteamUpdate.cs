namespace L4D2PlayStats.Hub.Core.Modules.Players.Models;

public class PlayerSteamUpdate(long communityId)
{
    public long CommunityId { get; } = communityId;
    public bool HasSummary { get; set; }
    public string? SteamName { get; set; }
    public DateTime? AccountCreatedAtUtc { get; set; }
    public bool HasBans { get; set; }
    public int? VacBans { get; set; }
    public int? GameBans { get; set; }
    public int? DaysSinceLastBan { get; set; }
    public bool HasPlaytime { get; set; }
    public int? Left4Dead2Minutes { get; set; }
    public int? Left4Dead2RecentMinutes { get; set; }
    public bool HasLevel { get; set; }
    public int? SteamLevel { get; set; }
    public bool Answered => HasSummary || HasBans || HasPlaytime || HasLevel;
}
