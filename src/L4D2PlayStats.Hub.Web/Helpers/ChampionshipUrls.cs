using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Championships;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static class ChampionshipUrls
{
    public static string Page(string slug)
    {
        return $"{AppConsts.ChampionshipsUrl}/{slug}";
    }

    public static string Series(string slug, int seriesId)
    {
        return $"{Page(slug)}/series/{seriesId.ToString(CultureInfo.InvariantCulture)}";
    }

    public static string Captain(string slug)
    {
        return $"{Page(slug)}/{AppConsts.ChampionshipCaptainPath}";
    }

    public static string Admin(int championshipId)
    {
        return $"{AppConsts.AdminChampionshipsUrl}/{championshipId.ToString(CultureInfo.InvariantCulture)}";
    }

    public static string AdminSeries(int championshipId, int seriesId)
    {
        return $"{Admin(championshipId)}/series/{seriesId.ToString(CultureInfo.InvariantCulture)}";
    }

    public static string? Image(int championshipId, string? file)
    {
        return string.IsNullOrEmpty(file) ? null : $"{AppConsts.ChampionshipImagesUrl}/{championshipId.ToString(CultureInfo.InvariantCulture)}/{file}";
    }

    public static string Flag(ChampionshipTeam team)
    {
        return Flag(team.Id, team.Flag);
    }

    public static string Flag(int teamId, string? flag)
    {
        return $"{AppConsts.ChampionshipFlagsUrl}/{teamId.ToString(CultureInfo.InvariantCulture)}?v={Version(flag)}";
    }

    public static string PendingFlag(ChampionshipTeam team)
    {
        return $"{AppConsts.ChampionshipFlagsUrl}/{team.Id.ToString(CultureInfo.InvariantCulture)}/pending?v={Version(team.PendingFlag)}";
    }

    private static string Version(string? file)
    {
        var name = file == null ? "0" : Path.GetFileNameWithoutExtension(file);

        return name.Length > AppConsts.ChampionshipFlagVersionLength ? name[^AppConsts.ChampionshipFlagVersionLength..] : name;
    }
}
