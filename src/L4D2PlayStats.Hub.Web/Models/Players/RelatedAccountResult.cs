using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Web.Helpers;

namespace L4D2PlayStats.Hub.Web.Models.Players;

public class RelatedAccountResult(PlayerConnectionResult player, bool linked, TimeZoneInfo timeZone) : SteamIdentityResult(long.Parse(player.CommunityId))
{
    public string Name { get; } = player.Name;
    public string LastSeen { get; } = Display.DateTime(player.LastConnectedAtUtc, timeZone);
    public long Connections { get; } = player.ConnectionCount;
    public bool Linked { get; } = linked;
}
