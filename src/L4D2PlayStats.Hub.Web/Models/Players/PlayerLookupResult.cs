namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PlayerLookupResult(long communityId, string? name, string? avatarUrl, bool knownOnSite) : SteamIdentityResult(communityId, avatarUrl)
{
    public string? Name { get; } = name;
    public bool KnownOnSite { get; } = knownOnSite;
}
