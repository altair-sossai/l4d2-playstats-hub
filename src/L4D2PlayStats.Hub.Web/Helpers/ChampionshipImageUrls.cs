using System.Globalization;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static class ChampionshipImageUrls
{
    public static string? Of(int championshipId, string? file)
    {
        return string.IsNullOrEmpty(file) ? null : $"{AppConsts.ChampionshipImagesUrl}/{championshipId.ToString(CultureInfo.InvariantCulture)}/{file}";
    }
}
