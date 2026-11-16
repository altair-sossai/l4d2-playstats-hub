using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Helpers;

public static class SteamIdParser
{
    private static readonly Regex SteamId = new(AppConsts.SteamIdPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex Steam3 = new(AppConsts.Steam3Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex CommunityId = new(AppConsts.SteamCommunityIdPattern, RegexOptions.Compiled);
    private static readonly Regex ProfileUrl = new(AppConsts.SteamProfileUrlPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static long? Parse(string? value)
    {
        var text = value?.Trim();

        if (string.IsNullOrEmpty(text))
            return null;

        var communityId = FromSteamId(text) ?? FromSteam3(text) ?? FromCommunityId(text);

        return communityId != null && SteamIds.IsValid(communityId.Value) ? communityId : null;
    }

    private static long? FromSteamId(string text)
    {
        var match = SteamId.Match(text);

        if (!match.Success || !long.TryParse(match.Groups[1].Value, out var server) || !long.TryParse(match.Groups[2].Value, out var account))
            return null;

        return account * 2 + AppConsts.SteamCommunityIdBase + server;
    }

    private static long? FromSteam3(string text)
    {
        var match = Steam3.Match(text);

        if (!match.Success || !long.TryParse(match.Groups[1].Value, out var account))
            return null;

        return account + AppConsts.SteamCommunityIdBase;
    }

    private static long? FromCommunityId(string text)
    {
        var match = CommunityId.Match(text);

        if (!match.Success)
            match = ProfileUrl.Match(text);

        return match.Success && long.TryParse(match.Groups[1].Value, out var communityId) ? communityId : null;
    }
}
