using System.Globalization;
using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Helpers;

public static class PlayerDisplayNames
{
    public static string Resolve(PlayerProfile profile)
    {
        return Named(profile.ForcedName) ?? Named(profile.MostUsedName) ?? Named(profile.SteamName) ?? Named(profile.Name) ?? Fallback(profile.CommunityId);
    }

    public static bool Refresh(PlayerProfile profile)
    {
        var name = Resolve(profile);

        if (name == profile.DisplayName)
            return false;

        profile.DisplayName = name;

        return true;
    }

    public static string Fallback(long communityId)
    {
        return communityId.ToString(CultureInfo.InvariantCulture);
    }

    public static string? Known(long communityId, string? displayName)
    {
        return string.IsNullOrWhiteSpace(displayName) || displayName == Fallback(communityId) ? null : displayName;
    }

    private static string? Named(string? name)
    {
        return Text.Clip(name, AppConsts.PlayerNameColumnMaxLength);
    }
}
