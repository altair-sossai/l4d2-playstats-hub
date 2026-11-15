using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Seasons;

public class Season
{
    public int Id { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }

    public bool Contains(DateTime date)
    {
        return date >= StartsAt && date < EndsAt;
    }
}

public static class SeasonCalendar
{
    public static int NormalizeLength(int lengthInMonths)
    {
        return Math.Clamp(lengthInMonths, 1, AppConsts.MaxSeasonLengthInMonths);
    }

    public static DateTime AlignedStart(DateTime date, int lengthInMonths)
    {
        var length = NormalizeLength(lengthInMonths);
        var block = (date.Month - 1) / length;

        return new DateTime(date.Year, 1, 1).AddMonths(block * length);
    }

    public static List<Season> Missing(IReadOnlyList<Season> existing, DateTime date, int lengthInMonths)
    {
        var length = NormalizeLength(lengthInMonths);
        var missing = new List<Season>();

        if (existing.Any(season => season.Contains(date)))
            return missing;

        if (existing.Count == 0)
        {
            var start = AlignedStart(date, length);
            missing.Add(new Season { StartsAt = start, EndsAt = start.AddMonths(length) });
            return missing;
        }

        var first = existing.MinBy(season => season.StartsAt)!;
        var last = existing.MaxBy(season => season.EndsAt)!;

        if (date >= last.EndsAt)
        {
            var start = last.EndsAt;

            while (start <= date)
            {
                missing.Add(new Season { StartsAt = start, EndsAt = start.AddMonths(length) });
                start = start.AddMonths(length);
            }

            return missing;
        }

        var end = first.StartsAt;

        while (end > date)
        {
            missing.Add(new Season { StartsAt = end.AddMonths(-length), EndsAt = end });
            end = end.AddMonths(-length);
        }

        return missing;
    }
}

public interface ISeasonService
{
    Task<List<Season>> ListAsync(CancellationToken cancellationToken = default);
    Task<Season?> FindAsync(int id, CancellationToken cancellationToken = default);
    Task<Season> GetOrCreateAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<Season> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<DateTime> SiteNowAsync(CancellationToken cancellationToken = default);
}

public class SeasonService(HubDbContext dbContext, ISettingsService settingsService, IClock clock) : ISeasonService
{
    public Task<List<Season>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Seasons.AsNoTracking().OrderByDescending(s => s.StartsAt).ToListAsync(cancellationToken);
    }

    public Task<Season?> FindAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.Seasons.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Season> GetOrCreateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var season = await dbContext.Seasons.FirstOrDefaultAsync(s => s.StartsAt <= date && s.EndsAt > date, cancellationToken);
        if (season != null)
            return season;

        var settings = await settingsService.GetAsync<SeasonSettings>(cancellationToken);
        var existing = await dbContext.Seasons.ToListAsync(cancellationToken);
        var missing = SeasonCalendar.Missing(existing, date, settings.LengthInMonths);

        dbContext.Seasons.AddRange(missing);
        await dbContext.SaveChangesAsync(cancellationToken);

        return missing.First(s => s.Contains(date));
    }

    public async Task<Season> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var now = await SiteNowAsync(cancellationToken);

        return await GetOrCreateAsync(now, cancellationToken);
    }

    public async Task<DateTime> SiteNowAsync(CancellationToken cancellationToken = default)
    {
        var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);
        var utcNow = DateTime.SpecifyKind(clock.UtcNow, DateTimeKind.Utc);

        if (!TimeZoneInfo.TryFindSystemTimeZoneById(site.TimeZone, out var timeZone))
            timeZone = TimeZoneInfo.Utc;

        return DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone), DateTimeKind.Unspecified);
    }
}
