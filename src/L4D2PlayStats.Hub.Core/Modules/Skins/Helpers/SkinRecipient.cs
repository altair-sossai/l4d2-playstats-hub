using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Helpers;

public static partial class SkinRecipient
{
    public static bool TryParse(string? value, out long communityId)
    {
        communityId = 0;

        var text = value?.Trim();
        if (string.IsNullOrEmpty(text))
            return false;

        var parsed = FromSteamId(text) ?? FromSteam3(text) ?? FromCommunityId(text);
        if (parsed == null || !SteamIds.IsValid(parsed.Value))
            return false;

        communityId = parsed.Value;

        return true;
    }

    private static long? FromSteamId(string value)
    {
        var match = SteamIdPattern().Match(value);
        if (!match.Success || !long.TryParse(match.Groups[1].Value, out var server) || !long.TryParse(match.Groups[2].Value, out var account))
            return null;

        return account * 2 + AppConsts.SteamCommunityIdBase + server;
    }

    private static long? FromSteam3(string value)
    {
        var match = Steam3Pattern().Match(value);

        return match.Success && long.TryParse(match.Groups[1].Value, out var account) ? account + AppConsts.SteamCommunityIdBase : null;
    }

    private static long? FromCommunityId(string value)
    {
        var match = CommunityIdPattern().Match(value);
        if (!match.Success)
            match = ProfilePattern().Match(value);

        return match.Success && long.TryParse(match.Groups[1].Value, out var communityId) ? communityId : null;
    }

    [GeneratedRegex(AppConsts.SteamIdPattern)]
    private static partial Regex SteamIdPattern();

    [GeneratedRegex(AppConsts.Steam3Pattern)]
    private static partial Regex Steam3Pattern();

    [GeneratedRegex(AppConsts.SteamCommunityIdPattern)]
    private static partial Regex CommunityIdPattern();

    [GeneratedRegex(AppConsts.SteamProfilePattern)]
    private static partial Regex ProfilePattern();
}
