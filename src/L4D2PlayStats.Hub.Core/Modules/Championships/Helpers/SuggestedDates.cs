namespace L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;

public static class SuggestedDates
{
    public static DateTime? Next(Championship championship, DateTime siteNow)
    {
        if (championship.SuggestedMinute == null)
            return null;

        var day = championship.StartsOn > siteNow ? championship.StartsOn.Value.Date : siteNow.Date;
        var date = day.AddMinutes(championship.SuggestedMinute.Value);

        while (date <= siteNow || championship.SuggestedWeekday != null && date.DayOfWeek != championship.SuggestedWeekday)
            date = date.AddDays(1);

        return championship.EndsOn != null && date.Date > championship.EndsOn.Value.Date ? null : date;
    }
}
