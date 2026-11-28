using L4D2PlayStats.Hub.Core.Modules.Championships;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static class ChampionshipFlags
{
    public static string Url(ChampionshipTeam team)
    {
        return $"{AppConsts.ChampionshipFlagsUrl}/{team.Id}?v={Version(team.Flag)}";
    }

    public static string PendingUrl(ChampionshipTeam team)
    {
        return $"{AppConsts.ChampionshipFlagsUrl}/{team.Id}/pending?v={Version(team.PendingFlag)}";
    }

    private static string Version(string? name)
    {
        return name == null ? "0" : Path.GetFileNameWithoutExtension(name)[..Math.Min(8, Path.GetFileNameWithoutExtension(name).Length)];
    }
}
