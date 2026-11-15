using System.Text.Json.Serialization;
using L4D2PlayStats.Hub.Core.Modules.Players;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models;

public class SiteChatMessage(SiteChatUser user, string text, DateTime when)
{
    [JsonIgnore]
    public long UserId { get; } = user.CommunityId;

    public DateTime When { get; } = when;
    public string Ticks => When.Ticks.ToString();
    public string SteamId { get; } = SteamIds.SteamId(user.CommunityId);
    public string CommunityId { get; } = user.CommunityId.ToString();
    public string ProfileUrl { get; } = SteamIds.ProfileUrl(user.CommunityId);
    public string Name { get; } = string.IsNullOrWhiteSpace(user.Name) ? user.CommunityId.ToString() : user.Name.Trim();
    public string Text { get; } = text;
}
