namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PlayerLookupResult
{
    public required string CommunityId { get; init; }
    public required string SteamId { get; init; }
    public required string Steam3 { get; init; }
    public required string ProfileUrl { get; init; }
    public string? Name { get; init; }
    public required string AvatarUrl { get; init; }
    public bool KnownOnSite { get; init; }
}
