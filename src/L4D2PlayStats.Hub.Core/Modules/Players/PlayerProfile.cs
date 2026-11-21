namespace L4D2PlayStats.Hub.Core.Modules.Players;

public class PlayerProfile
{
    public long CommunityId { get; set; }
    public string? Name { get; set; }
    public string? MostUsedName { get; set; }
    public string? DisplayName { get; set; }
    public string? ForcedName { get; set; }
    public DateTime? ForcedNameUpdatedAtUtc { get; set; }
    public long? ForcedNameUpdatedBy { get; set; }
    public DateTime FirstSeenAtUtc { get; set; }
    public DateTime LastSeenAtUtc { get; set; }
    public DateTime? AvatarUpdatedAtUtc { get; set; }
    public string? SteamName { get; set; }
    public DateTime? AccountCreatedAtUtc { get; set; }
    public int? VacBans { get; set; }
    public int? GameBans { get; set; }
    public int? DaysSinceLastBan { get; set; }
    public int? Left4Dead2Minutes { get; set; }
    public int? Left4Dead2RecentMinutes { get; set; }
    public int? SteamLevel { get; set; }
    public DateTime? SteamUpdatedAtUtc { get; set; }
}
