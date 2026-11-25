using System.Globalization;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Infrastructure.Extensions;
using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services;

public class ChampionshipLogService(HubDbContext dbContext, IClock clock) : IChampionshipLogService
{
    public void Add(int championshipId, long? communityId, string text, params object?[] values)
    {
        var joined = values.Length == 0 ? null : string.Join(AppConsts.ChampionshipLogValueSeparator, values.Select(value => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty));

        if (joined is { Length: > AppConsts.ChampionshipLogValuesMaxLength })
            joined = joined[..AppConsts.ChampionshipLogValuesMaxLength];

        dbContext.ChampionshipLogs.Add(new ChampionshipLog
        {
            ChampionshipId = championshipId,
            CommunityId = communityId,
            Text = text,
            Values = joined,
            CreatedAtUtc = clock.UtcNow
        });
    }

    public Task<Page<ChampionshipLog>> PageAsync(int championshipId, int page, int size, CancellationToken cancellationToken = default)
    {
        return dbContext.ChampionshipLogs.AsNoTracking()
            .Where(log => log.ChampionshipId == championshipId)
            .OrderByDescending(log => log.CreatedAtUtc)
            .ThenByDescending(log => log.Id)
            .ToPageAsync(page, size, cancellationToken);
    }
}
