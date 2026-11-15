using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Seasons.Services;

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
