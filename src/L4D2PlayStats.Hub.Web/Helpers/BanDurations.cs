namespace L4D2PlayStats.Hub.Web.Helpers;

public static class BanDurations
{
    public static bool IsValid(string? duration)
    {
        return duration == AppConsts.BanPermanentDuration
               || duration == AppConsts.BanCustomDuration
               || (int.TryParse(duration, out var minutes) && AppConsts.BanDurationsInMinutes.Contains(minutes));
    }

    public static bool TryEndsAt(string? duration, string? endsAt, TimeZoneInfo timeZone, DateTime now, out DateTime? endsAtUtc)
    {
        endsAtUtc = null;

        if (duration == AppConsts.BanPermanentDuration)
            return true;

        if (duration == AppConsts.BanCustomDuration)
        {
            endsAtUtc = ChampionshipDates.ToUtc(endsAt, timeZone);

            return endsAtUtc > now;
        }

        if (!int.TryParse(duration, out var minutes) || !AppConsts.BanDurationsInMinutes.Contains(minutes))
            return false;

        endsAtUtc = now.AddMinutes(minutes);

        return true;
    }
}
