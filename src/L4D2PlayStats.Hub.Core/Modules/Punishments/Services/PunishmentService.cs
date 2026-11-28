using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Enums;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Models;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Punishments.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Punishments.Services;

public class PunishmentService(HubDbContext dbContext, HubState hubState, IClock clock, INotificationService notifications) : IPunishmentService
{
    public async Task<Dictionary<long, int>> TotalsAsync(int seasonId, CancellationToken cancellationToken = default)
    {
        var punishments = await dbContext.Punishments
            .AsNoTracking()
            .Where(p => p.SeasonId == seasonId)
            .Select(p => new { p.CommunityId, p.Points })
            .ToListAsync(cancellationToken);

        return punishments
            .GroupBy(p => p.CommunityId)
            .ToDictionary(group => group.Key, group => group.Sum(p => p.Points));
    }

    public Task<List<Punishment>> ListAsync(int seasonId, CancellationToken cancellationToken = default)
    {
        return dbContext.Punishments.AsNoTracking().Where(p => p.SeasonId == seasonId).OrderByDescending(p => p.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public async Task<Punishment?> AddAsync(int seasonId, long communityId, int points, string? reason, long? createdBy, CancellationToken cancellationToken = default)
    {
        if (points <= 0)
            return null;

        var punishment = new Punishment
        {
            SeasonId = seasonId,
            CommunityId = communityId,
            Points = points,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            CreatedByCommunityId = createdBy,
            CreatedAtUtc = clock.UtcNow
        };

        dbContext.Punishments.Add(punishment);
        notifications.Send(communityId, new NotificationMessage(NotificationTopic.Punishments, "You lost {0} experience points in this season.", PlayerPath(communityId), points));
        await dbContext.SaveChangesAsync(cancellationToken);
        hubState.DataChanged();

        return punishment;
    }

    public async Task<int> RemoveAsync(int seasonId, long communityId, CancellationToken cancellationToken = default)
    {
        var punishments = await dbContext.Punishments.Where(p => p.SeasonId == seasonId && p.CommunityId == communityId).ToListAsync(cancellationToken);

        dbContext.Punishments.RemoveRange(punishments);

        if (punishments.Count > 0)
            notifications.Send(communityId, new NotificationMessage(NotificationTopic.Punishments, "Your punishments in this season were removed.", PlayerPath(communityId)));

        await dbContext.SaveChangesAsync(cancellationToken);
        hubState.DataChanged();

        return punishments.Count;
    }

    private static string PlayerPath(long communityId)
    {
        return $"{AppConsts.PlayerPathPrefix}{communityId.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
    }
}
