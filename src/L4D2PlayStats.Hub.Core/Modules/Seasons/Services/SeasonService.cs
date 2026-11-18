using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Seasons.Services;

public class SeasonService(HubDbContext dbContext, HubState hubState, ISettingsService settingsService, IClock clock) : ISeasonService
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
        var season = await ContainingAsync(date, cancellationToken);
        if (season != null)
            return season;

        await hubState.SeasonLock.WaitAsync(cancellationToken);

        try
        {
            return await ContainingAsync(date, cancellationToken) ?? await CreateAsync(date, cancellationToken);
        }
        finally
        {
            hubState.SeasonLock.Release();
        }
    }

    public async Task<Season> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var now = await SiteNowAsync(cancellationToken);

        return await GetOrCreateAsync(now, cancellationToken);
    }

    public async Task<DateTime> SiteNowAsync(CancellationToken cancellationToken = default)
    {
        var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);

        return DateTime.SpecifyKind(SiteTime.FromUtc(clock.UtcNow, SiteTime.Zone(site.TimeZone)), DateTimeKind.Unspecified);
    }

    private Task<Season?> ContainingAsync(DateTime date, CancellationToken cancellationToken)
    {
        return dbContext.Seasons.FirstOrDefaultAsync(s => s.StartsAt <= date && s.EndsAt > date, cancellationToken);
    }

    private async Task<Season> CreateAsync(DateTime date, CancellationToken cancellationToken)
    {
        var settings = await settingsService.GetAsync<SeasonSettings>(cancellationToken);
        var existing = await dbContext.Seasons.ToListAsync(cancellationToken);
        var missing = SeasonCalendar.Missing(existing, date, settings.LengthInMonths);

        dbContext.Seasons.AddRange(missing);
        await dbContext.SaveChangesAsync(cancellationToken);

        return missing.First(s => s.Contains(date));
    }
}
