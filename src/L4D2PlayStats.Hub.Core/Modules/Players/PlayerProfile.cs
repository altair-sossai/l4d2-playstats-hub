namespace L4D2PlayStats.Hub.Core.Modules.Players;

public class PlayerProfile
{
    public long CommunityId { get; set; }
    public string? Name { get; set; }
    public DateTime FirstSeenAtUtc { get; set; }
    public DateTime LastSeenAtUtc { get; set; }
    public DateTime? AvatarUpdatedAtUtc { get; set; }
}

public static class SteamIds
{
    public static string ProfileUrl(long communityId)
    {
        return $"{AppConsts.SteamProfileUrl}{communityId}";
    }

    public static string SteamId(long communityId)
    {
        var accountId = communityId - AppConsts.SteamCommunityIdBase;

        return $"STEAM_1:{accountId % 2}:{accountId / 2}";
    }

    public static string Steam3(long communityId)
    {
        return $"[U:1:{communityId - AppConsts.SteamCommunityIdBase}]";
    }

    public static bool IsValid(long communityId)
    {
        return communityId > AppConsts.SteamCommunityIdBase;
    }
}
