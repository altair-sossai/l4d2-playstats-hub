namespace L4D2PlayStats.Hub.Core.Modules.Players.Models;

public record PlayerSteamDetails(
    long CommunityId,
    string? SteamName,
    DateTime? AccountCreatedAtUtc,
    int? VacBans,
    int? GameBans,
    int? DaysSinceLastBan,
    int? Left4Dead2Minutes,
    int? Left4Dead2RecentMinutes,
    int? SteamLevel,
    DateTime? SteamUpdatedAtUtc);
