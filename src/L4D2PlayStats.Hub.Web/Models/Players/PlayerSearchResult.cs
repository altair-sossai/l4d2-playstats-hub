using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Web.Helpers;

namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PlayerSearchResult(PlayerDirectoryEntry entry, TimeZoneInfo timeZone) : SteamIdentityResult(entry.CommunityId)
{
    public string? Name { get; } = entry.Name;
    public List<string> PreviousNames { get; } = [.. entry.Names.Where(name => name != entry.Name).Take(AppConsts.PlayerPickerPreviousNames)];
    public int? Position { get; } = entry.Position;
    public bool Ranked { get; } = entry.Ranked;
    public bool KnownOnSite { get; } = entry.Known;
    public DateTime? LastSeenAtUtc { get; } = entry.LastSeenAtUtc;
    public string? LastSeen { get; } = entry.LastSeenAtUtc == null ? null : Display.DateTime(entry.LastSeenAtUtc.Value, timeZone);
}
