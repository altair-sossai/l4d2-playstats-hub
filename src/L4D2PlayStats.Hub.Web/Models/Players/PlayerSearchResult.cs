using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Web.Helpers;

namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PlayerSearchResult(PlayerDirectoryEntry entry, TimeZoneInfo timeZone)
{
    public string CommunityId { get; } = entry.CommunityId.ToString(CultureInfo.InvariantCulture);
    public string SteamId { get; } = SteamIds.SteamId(entry.CommunityId);
    public string Steam3 { get; } = SteamIds.Steam3(entry.CommunityId);
    public string ProfileUrl { get; } = SteamIds.ProfileUrl(entry.CommunityId);
    public string? Name { get; } = entry.Name;
    public List<string> PreviousNames { get; } = [.. entry.Names.Where(name => name != entry.Name).Take(AppConsts.PlayerPickerPreviousNames)];
    public string AvatarUrl { get; } = $"{AppConsts.AvatarUrl}{entry.CommunityId}";
    public int? Position { get; } = entry.Position;
    public bool Ranked { get; } = entry.Ranked;
    public bool KnownOnSite { get; } = entry.Known;
    public DateTime? LastSeenAtUtc { get; } = entry.LastSeenAtUtc;
    public string? LastSeen { get; } = entry.LastSeenAtUtc == null ? null : LiveDisplay.ShortDateTime(entry.LastSeenAtUtc.Value, timeZone);
}