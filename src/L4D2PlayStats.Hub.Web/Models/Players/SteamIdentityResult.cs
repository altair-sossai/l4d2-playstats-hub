using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Web.Helpers;

namespace L4D2PlayStats.Hub.Web.Models.Players;

public class SteamIdentityResult(long communityId, string? avatarUrl = null) : SteamIdentity(communityId)
{
    public string AvatarUrl { get; } = avatarUrl ?? PlayerUrls.Avatar(communityId);
}
