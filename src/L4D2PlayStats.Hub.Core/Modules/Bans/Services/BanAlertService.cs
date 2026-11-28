using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Infrastructure.Extensions;
using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Bans.Enums;
using L4D2PlayStats.Hub.Core.Modules.Bans.Extensions;
using L4D2PlayStats.Hub.Core.Modules.Bans.Models;
using L4D2PlayStats.Hub.Core.Modules.Bans.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Enums;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Models;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Bans.Services;

public class BanAlertService(HubDbContext dbContext, IClock clock, ISettingsService settingsService, IPlayerNameService playerNameService, INotificationService notifications) : IBanAlertService
{
    public async Task<int> DetectAsync(long communityId, CancellationToken cancellationToken = default)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);

        if (!modules.IsEnabled(HubModule.Bans))
            return 0;

        var connections = await SharedAsync(communityId, cancellationToken);
        var mine = connections.Where(connection => connection.CommunityId == communityId).ToList();
        var others = connections.Where(connection => connection.CommunityId != communityId).GroupBy(connection => connection.CommunityId).ToList();

        if (others.Count == 0)
            return 0;

        var bans = await ActiveAsync([communityId, .. others.Select(group => group.Key)], cancellationToken);
        var found = new List<(Ban Ban, long CommunityId)>();

        if (bans.TryGetValue(communityId, out var own))
            found.AddRange(others.Where(group => !bans.ContainsKey(group.Key) && LinkedAt(mine, group) > own.CreatedAtUtc).Select(group => (own, group.Key)));
        else
            found.AddRange(others.Where(group => bans.TryGetValue(group.Key, out var ban) && LinkedAt(mine, group) > ban.CreatedAtUtc).Select(group => (bans[group.Key], communityId)));

        return await CreateAsync(found, cancellationToken);
    }

    public async Task<int> LinkedAsync(Ban ban, CancellationToken cancellationToken = default)
    {
        var connections = await SharedAsync(ban.CommunityId, cancellationToken);
        var others = connections.Select(connection => connection.CommunityId).Where(id => id != ban.CommunityId).Distinct().ToList();

        if (others.Count == 0)
            return 0;

        var banned = await ActiveAsync(others, cancellationToken);

        return await CreateAsync([.. others.Where(id => !banned.ContainsKey(id)).Select(id => (ban, id))], cancellationToken);
    }

    public async Task<Page<BanAlertEntry>> PageAsync(int page, int size, CancellationToken cancellationToken = default)
    {
        var rows = await Rows(Open())
            .OrderByDescending(row => row.Alert.CreatedAtUtc)
            .ThenByDescending(row => row.Alert.Id)
            .ToPageAsync(page, size, cancellationToken);
        var names = await NamesAsync([.. rows.Items.Select(row => row.Alert.CommunityId)], cancellationToken);

        return rows.Map(row => new BanAlertEntry(row.Alert, row.Ban, names.GetValueOrDefault(row.Alert.CommunityId)));
    }

    public Task<int> OpenCountAsync(CancellationToken cancellationToken = default)
    {
        return Open().CountAsync(cancellationToken);
    }

    public async Task<BanAlertEntry?> FindAsync(int alertId, CancellationToken cancellationToken = default)
    {
        var row = await Rows(Open().Where(alert => alert.Id == alertId)).FirstOrDefaultAsync(cancellationToken);

        if (row == null)
            return null;

        var names = await NamesAsync([row.Alert.CommunityId], cancellationToken);

        return new BanAlertEntry(row.Alert, row.Ban, names.GetValueOrDefault(row.Alert.CommunityId));
    }

    public async Task<bool> IgnoreAsync(int alertId, long? ignoredBy, CancellationToken cancellationToken = default)
    {
        var alert = await dbContext.Set<BanAlert>().FirstOrDefaultAsync(item => item.Id == alertId && item.Status == BanAlertStatus.Open, cancellationToken);

        if (alert == null)
            return false;

        Close(alert, BanAlertStatus.Ignored, ignoredBy);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<int> ResolveAsync(long communityId, long? bannedBy, CancellationToken cancellationToken = default)
    {
        var alerts = await dbContext.Set<BanAlert>().Where(alert => alert.CommunityId == communityId && alert.Status == BanAlertStatus.Open).ToListAsync(cancellationToken);

        foreach (var alert in alerts)
            Close(alert, BanAlertStatus.Banned, bannedBy);

        if (alerts.Count > 0)
            await dbContext.SaveChangesAsync(cancellationToken);

        return alerts.Count;
    }

    private IQueryable<BanAlert> Open()
    {
        var active = dbContext.Set<Ban>().Active(clock.UtcNow).Select(ban => ban.Id);

        return dbContext.Set<BanAlert>().AsNoTracking().Where(alert => alert.Status == BanAlertStatus.Open && active.Contains(alert.BanId));
    }

    private IQueryable<BanAlertRow> Rows(IQueryable<BanAlert> alerts)
    {
        return alerts.Join(dbContext.Set<Ban>().AsNoTracking(), alert => alert.BanId, ban => ban.Id, (alert, ban) => new BanAlertRow { Alert = alert, Ban = ban });
    }

    private void Close(BanAlert alert, BanAlertStatus status, long? by)
    {
        alert.Status = status;
        alert.ResolvedByCommunityId = by;
        alert.ResolvedAtUtc = clock.UtcNow;
    }

    private async Task<int> CreateAsync(List<(Ban Ban, long CommunityId)> found, CancellationToken cancellationToken)
    {
        if (found.Count == 0)
            return 0;

        var banIds = found.Select(item => item.Ban.Id).Distinct().ToList();
        var existing = await dbContext.Set<BanAlert>()
            .AsNoTracking()
            .Where(alert => banIds.Contains(alert.BanId))
            .Select(alert => new { alert.BanId, alert.CommunityId })
            .ToListAsync(cancellationToken);
        var known = existing.Select(alert => (alert.BanId, alert.CommunityId)).ToHashSet();
        var now = clock.UtcNow;
        var created = 0;

        foreach (var (ban, communityId) in found)
        {
            if (!known.Add((ban.Id, communityId)))
                continue;

            dbContext.Set<BanAlert>().Add(new BanAlert { BanId = ban.Id, CommunityId = communityId, Status = BanAlertStatus.Open, CreatedAtUtc = now });
            notifications.SendAdministrators(new NotificationMessage(NotificationTopic.Bans, "{0} shares an IP address with {1}, who is banned from the game servers.", AppConsts.BansAlertsPath, communityId, ban.CommunityId));
            created++;
        }

        if (created > 0)
            await dbContext.SaveChangesAsync(cancellationToken);

        return created;
    }

    private async Task<Dictionary<long, Ban>> ActiveAsync(List<long> communityIds, CancellationToken cancellationToken)
    {
        var bans = await dbContext.Set<Ban>()
            .AsNoTracking()
            .Active(clock.UtcNow)
            .Where(ban => communityIds.Contains(ban.CommunityId))
            .ToListAsync(cancellationToken);

        return bans.GroupBy(ban => ban.CommunityId).ToDictionary(group => group.Key, group => group.OrderBy(ban => ban.CreatedAtUtc).First());
    }

    private Task<List<PlayerConnection>> SharedAsync(long communityId, CancellationToken cancellationToken)
    {
        var addresses = dbContext.Set<PlayerConnection>().Where(connection => connection.CommunityId == communityId).Select(connection => connection.IpAddress);

        return dbContext.Set<PlayerConnection>()
            .AsNoTracking()
            .Where(connection => addresses.Contains(connection.IpAddress))
            .ToListAsync(cancellationToken);
    }

    private async Task<Dictionary<long, string>> NamesAsync(List<long> communityIds, CancellationToken cancellationToken)
    {
        return communityIds.Count == 0 ? [] : await playerNameService.NamesAsync(communityIds, cancellationToken);
    }

    private static DateTime LinkedAt(List<PlayerConnection> mine, IEnumerable<PlayerConnection> theirs)
    {
        return theirs
            .Join(mine, their => their.IpAddress, own => own.IpAddress, (their, own) => their.FirstSeenAtUtc > own.FirstSeenAtUtc ? their.FirstSeenAtUtc : own.FirstSeenAtUtc)
            .DefaultIfEmpty(DateTime.MinValue)
            .Min();
    }
}
