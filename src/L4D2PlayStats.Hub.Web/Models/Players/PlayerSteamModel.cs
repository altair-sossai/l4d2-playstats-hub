using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Web.Helpers;

namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PlayerSteamModel(PlayerSteamDetails? details, TimeZoneInfo timeZone, DateTime nowUtc)
{
    public string? TotalHours { get; } = Hours(details?.Left4Dead2Minutes);
    public string? RecentHours { get; } = Hours(details?.Left4Dead2RecentMinutes);
    public string? AccountCreated { get; } = details?.AccountCreatedAtUtc == null ? null : Display.Date(details.AccountCreatedAtUtc.Value, timeZone);
    public int? AccountAgeInYears { get; } = details?.AccountCreatedAtUtc == null ? null : Years(details.AccountCreatedAtUtc.Value, nowUtc);
    public int? SteamLevel { get; } = details?.SteamLevel;
    public int? VacBans { get; } = details?.VacBans;
    public int? GameBans { get; } = details?.GameBans;
    public int? DaysSinceLastBan { get; } = LastBan(details, nowUtc);

    private static string? Hours(int? minutes)
    {
        return minutes == null ? null : (minutes.Value / (decimal)AppConsts.MinutesPerHour).ToString(AppConsts.SteamHoursFormat, CultureInfo.CurrentCulture);
    }

    private static int Years(DateTime createdAtUtc, DateTime nowUtc)
    {
        var years = nowUtc.Year - createdAtUtc.Year;

        if (nowUtc < createdAtUtc.AddYears(years))
            years--;

        return Math.Max(0, years);
    }

    private static int? LastBan(PlayerSteamDetails? details, DateTime nowUtc)
    {
        if (details?.DaysSinceLastBan == null || (details.VacBans ?? 0) + (details.GameBans ?? 0) == 0)
            return null;

        var elapsed = details.SteamUpdatedAtUtc == null ? 0 : Math.Max(0, (int)(nowUtc - details.SteamUpdatedAtUtc.Value).TotalDays);

        return details.DaysSinceLastBan.Value + elapsed;
    }
}
