using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Models;

public class SteamIdentity(long communityId)
{
    public string CommunityId { get; } = communityId.ToString(CultureInfo.InvariantCulture);
    public string SteamId { get; } = SteamIds.SteamId(communityId);
    public string Steam3 { get; } = SteamIds.Steam3(communityId);
    public string ProfileUrl { get; } = SteamIds.ProfileUrl(communityId);
}
