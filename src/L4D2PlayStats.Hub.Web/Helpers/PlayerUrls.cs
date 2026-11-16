using L4D2PlayStats.Hub.Web.Models.Players;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static class PlayerUrls
{
    public static string Details(long communityId, long? compareWith = null, int? seasonId = null)
    {
        var path = compareWith == null
            ? $"{AppConsts.PlayerUrl}/{communityId}"
            : $"{AppConsts.PlayerUrl}/{communityId}/{compareWith}";

        return path + Season(seasonId, '?');
    }

    public static string List(PlayerResultProperty? orderBy = null, bool? asc = null, int? seasonId = null)
    {
        if (orderBy == null || asc == null)
            return AppConsts.PlayersUrl + Season(seasonId, '?');

        return $"{AppConsts.PlayersUrl}?orderBy={orderBy}&asc={asc}" + Season(seasonId, '&');
    }

    public static string CampaignThumb(string? campaign)
    {
        var name = string.IsNullOrEmpty(campaign) ? AppConsts.PlayerCampaignThumbFallback : campaign;
        var slug = string.Concat(name.ToLowerInvariant().Replace(' ', '-').Where(c => c is >= 'a' and <= 'z' or >= '0' and <= '9' or '-'));

        return $"{AppConsts.PlayerCampaignThumbFolder}{slug}{AppConsts.PlayerCampaignThumbExtension}";
    }

    private static string Season(int? seasonId, char separator)
    {
        return seasonId == null ? string.Empty : $"{separator}{AppConsts.PlayerSeasonParameter}={seasonId}";
    }
}
