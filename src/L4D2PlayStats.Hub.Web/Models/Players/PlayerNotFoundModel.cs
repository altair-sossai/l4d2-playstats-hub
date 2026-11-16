using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;

namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PlayerNotFoundModel(long communityId, PlayerConnectionResult? player, IReadOnlyList<PlayerConnectionResult> relatedPlayers, string? knownName = null)
{
    private readonly bool _valid = communityId > 0;

    public long CommunityId { get; } = communityId;
    public string? Name { get; } = player?.Name ?? knownName;
    public string? SteamId => _valid ? SteamIds.DisplaySteamId(CommunityId) : null;
    public string? ProfileUrl => player?.ProfileUrl ?? (_valid ? SteamIds.ProfileUrl(CommunityId) : null);
    public DateTime? LastConnectedAtUtc { get; } = player?.LastConnectedAtUtc;
    public long? ConnectionCount { get; } = player?.ConnectionCount;
    public IReadOnlyList<PlayerConnectionResult> RelatedPlayers { get; } = relatedPlayers;
    public PlayerPeriodModel? Period { get; init; }
    public TimeZoneInfo TimeZone { get; init; } = TimeZoneInfo.Utc;

    public DateTime LocalTime(DateTime utc)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), TimeZone);
    }
}
