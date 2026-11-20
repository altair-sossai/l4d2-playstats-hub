using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.AntiCheat.Extensions;

public static class AntiCheatAccessExtensions
{
    public static string ToName(this AntiCheatAccessMode mode)
    {
        return mode == AntiCheatAccessMode.Everyone ? AppConsts.AntiCheatModeEveryone : AppConsts.AntiCheatModeListed;
    }

    public static string ToName(this AntiCheatAccessList list)
    {
        return list == AntiCheatAccessList.Whitelist ? AppConsts.AntiCheatWhitelistName : AppConsts.AntiCheatRequiredName;
    }

    public static AntiCheatAccessList ListOf(this AntiCheatAccessMode mode)
    {
        return mode == AntiCheatAccessMode.Everyone ? AntiCheatAccessList.Whitelist : AntiCheatAccessList.Required;
    }

    public static AntiCheatAccessList Other(this AntiCheatAccessList list)
    {
        return list == AntiCheatAccessList.Whitelist ? AntiCheatAccessList.Required : AntiCheatAccessList.Whitelist;
    }
}