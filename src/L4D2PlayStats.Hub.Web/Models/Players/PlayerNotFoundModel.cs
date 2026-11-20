using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Helpers;

namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PlayerNotFoundModel(long communityId, PlayerConnectionResult? player, IReadOnlyList<PlayerConnectionResult> relatedPlayers, string? knownName = null)
{
    public long CommunityId { get; } = communityId;
    public string? Name { get; } = player?.Name ?? knownName;
    public string SteamId => SteamIds.DisplaySteamId(CommunityId);
    public string GameSteamId => SteamIds.SteamId(CommunityId);
    public string Steam3 => SteamIds.Steam3(CommunityId);
    public string ProfileUrl => player?.ProfileUrl ?? SteamIds.ProfileUrl(CommunityId);
    public DateTime? LastConnectedAtUtc { get; } = player?.LastConnectedAtUtc;
    public long? ConnectionCount { get; } = player?.ConnectionCount;
    public IReadOnlyList<PlayerConnectionResult> RelatedPlayers { get; } = relatedPlayers;
    public PlayerPeriodModel? Period { get; init; }
    public bool Ranking => Period != null;
    public TimeZoneInfo TimeZone { get; init; } = TimeZoneInfo.Utc;
    public PlayerCommunityModel? Community { get; set; }

    public DateTime LocalTime(DateTime utc)
    {
        return SiteTime.FromUtc(utc, TimeZone);
    }
}
