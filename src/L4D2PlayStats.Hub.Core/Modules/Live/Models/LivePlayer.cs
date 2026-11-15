using System.Text.Json.Serialization;
using L4D2PlayStats.Hub.Core.Modules.Live.Enums;
using L4D2PlayStats.Hub.Core.Modules.Players;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models;

public class LivePlayer
{
    [JsonIgnore]
    public long? Id { get; private set; }

    public string? CommunityId
    {
        get => Id?.ToString();
        set => Id = long.TryParse(value, out var communityId) && SteamIds.IsValid(communityId) ? communityId : null;
    }

    public string? SteamId => Id == null ? null : SteamIds.SteamId(Id.Value);
    public string? Steam3 => Id == null ? null : SteamIds.Steam3(Id.Value);
    public string? ProfileUrl => Id == null ? null : SteamIds.ProfileUrl(Id.Value);
    public string? Name { get; set; }
    public bool IsBot { get; set; }
    public bool IsAdmin { get; set; }
    public decimal? Latency { get; set; }

    public LatencyType? LatencyType => Latency switch
    {
        null => null,
        < AppConsts.LiveLatencyLowLimit => Enums.LatencyType.Low,
        < AppConsts.LiveLatencyMediumLimit => Enums.LatencyType.Medium,
        _ => Enums.LatencyType.High
    };
}
