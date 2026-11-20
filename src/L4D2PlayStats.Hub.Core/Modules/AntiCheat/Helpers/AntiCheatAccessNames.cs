using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Enums;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Extensions;

namespace L4D2PlayStats.Hub.Core.Modules.AntiCheat.Helpers;

public static class AntiCheatAccessNames
{
    public static bool TryParseMode(string? value, out AntiCheatAccessMode mode)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case AppConsts.AntiCheatModeEveryone:
                mode = AntiCheatAccessMode.Everyone;
                return true;
            case AppConsts.AntiCheatModeListed:
                mode = AntiCheatAccessMode.Listed;
                return true;
            default:
                mode = AppConsts.AntiCheatDefaultMode;
                return false;
        }
    }

    public static bool TryParseList(string? value, out AntiCheatAccessList list)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case AppConsts.AntiCheatWhitelistName:
                list = AntiCheatAccessList.Whitelist;
                return true;
            case AppConsts.AntiCheatRequiredName:
                list = AntiCheatAccessList.Required;
                return true;
            default:
                list = AppConsts.AntiCheatDefaultMode.ListOf();
                return false;
        }
    }
}
