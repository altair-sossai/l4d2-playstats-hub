using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Helpers;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static class LiveDisplay
{
    public static string? PlayerUrl(LivePlayer? player)
    {
        return player?.Id == null ? null : PlayerUrls.Details(player.Id.Value);
    }

    public static string Avatar(LivePlayer player)
    {
        return player.Id == null ? AppConsts.EmptyAvatarUrl : $"{AppConsts.AvatarUrl}{player.Id}";
    }

    public static string Width(decimal fraction)
    {
        return $"{Math.Round(fraction * 100, 2).ToString(CultureInfo.InvariantCulture)}%";
    }

    public static string Time(DateTime utc, TimeZoneInfo timeZone)
    {
        var local = SiteTime.FromUtc(utc, timeZone);

        return local.ToString(AppConsts.ShortTimeFormat, CultureInfo.InvariantCulture);
    }

    public static string ShortDateTime(DateTime utc, TimeZoneInfo timeZone)
    {
        var local = SiteTime.FromUtc(utc, timeZone);

        return local.ToString(AppConsts.ShortDateTimeFormat, CultureInfo.CurrentUICulture);
    }
}