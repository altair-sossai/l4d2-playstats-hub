using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Helpers;

public static class SkinRecipient
{
    public static bool TryParse(string? value, out long communityId)
    {
        var parsed = SteamIdParser.Parse(value);

        communityId = parsed ?? 0;

        return parsed != null;
    }
}