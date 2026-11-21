namespace L4D2PlayStats.Hub.Web.Infrastructure.Steam;

public record SteamBans(long CommunityId, int VacBans, int GameBans, int DaysSinceLastBan);
