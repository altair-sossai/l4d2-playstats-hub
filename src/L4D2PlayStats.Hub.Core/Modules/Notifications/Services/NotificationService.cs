using System.Globalization;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Infrastructure.Extensions;
using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Enums;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Models;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Notifications.Services;

public class NotificationService(HubDbContext dbContext, IClock clock) : INotificationService
{
    public void Send(long communityId, NotificationMessage message)
    {
        Add(NotificationAudience.Player, communityId, message);
    }

    public void Send(IEnumerable<long> communityIds, NotificationMessage message)
    {
        foreach (var communityId in communityIds.Where(id => id > 0).Distinct())
            Add(NotificationAudience.Player, communityId, message);
    }

    public void SendAdministrators(NotificationMessage message)
    {
        Add(NotificationAudience.Administrators, null, message);
    }

    public void SendEveryone(NotificationMessage message)
    {
        Add(NotificationAudience.Everyone, null, message);
    }

    public async Task<Page<NotificationItem>> PageAsync(long communityId, bool administrator, int page, int size, CancellationToken cancellationToken = default)
    {
        var rows = await Visible(communityId, administrator)
            .OrderByDescending(notification => notification.CreatedAtUtc)
            .ThenByDescending(notification => notification.Id)
            .ToPageAsync(page, size, cancellationToken);
        var read = await ReadIdsAsync(communityId, rows.Items.Select(notification => notification.Id).ToList(), cancellationToken);

        return rows.Map(notification => Item(notification, read.Contains(notification.Id)));
    }

    public async Task<List<NotificationItem>> LatestAsync(long communityId, bool administrator, int count, CancellationToken cancellationToken = default)
    {
        var rows = await Visible(communityId, administrator)
            .OrderByDescending(notification => notification.CreatedAtUtc)
            .ThenByDescending(notification => notification.Id)
            .Take(count)
            .ToListAsync(cancellationToken);
        var read = await ReadIdsAsync(communityId, rows.Select(notification => notification.Id).ToList(), cancellationToken);

        return rows.Select(notification => Item(notification, read.Contains(notification.Id))).ToList();
    }

    public Task<int> UnreadAsync(long communityId, bool administrator, CancellationToken cancellationToken = default)
    {
        return Unread(communityId, administrator).CountAsync(cancellationToken);
    }

    public async Task<NotificationItem?> OpenAsync(long communityId, bool administrator, int notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await Visible(communityId, administrator).FirstOrDefaultAsync(item => item.Id == notificationId, cancellationToken);

        if (notification == null)
            return null;

        if (!await dbContext.NotificationReads.AnyAsync(read => read.NotificationId == notificationId && read.CommunityId == communityId, cancellationToken))
        {
            dbContext.NotificationReads.Add(new NotificationRead { NotificationId = notificationId, CommunityId = communityId, ReadAtUtc = clock.UtcNow });
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Item(notification, true);
    }

    public async Task<int> ReadAllAsync(long communityId, bool administrator, CancellationToken cancellationToken = default)
    {
        var ids = await Unread(communityId, administrator).Select(notification => notification.Id).ToListAsync(cancellationToken);
        var now = clock.UtcNow;

        dbContext.NotificationReads.AddRange(ids.Select(id => new NotificationRead { NotificationId = id, CommunityId = communityId, ReadAtUtc = now }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return ids.Count;
    }

    public Task<int> CleanupAsync(CancellationToken cancellationToken = default)
    {
        var before = clock.UtcNow.AddDays(-AppConsts.NotificationRetentionInDays);

        return dbContext.Notifications.Where(notification => notification.CreatedAtUtc < before).ExecuteDeleteAsync(cancellationToken);
    }

    private IQueryable<Notification> Visible(long communityId, bool administrator)
    {
        return dbContext.Notifications.AsNoTracking().Where(notification =>
            notification.Audience == NotificationAudience.Everyone
            || (notification.Audience == NotificationAudience.Administrators && administrator)
            || (notification.Audience == NotificationAudience.Player && notification.CommunityId == communityId));
    }

    private IQueryable<Notification> Unread(long communityId, bool administrator)
    {
        return Visible(communityId, administrator).Where(notification => !dbContext.NotificationReads.Any(read => read.NotificationId == notification.Id && read.CommunityId == communityId));
    }

    private async Task<HashSet<int>> ReadIdsAsync(long communityId, List<int> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
            return [];

        return (await dbContext.NotificationReads.AsNoTracking()
            .Where(read => read.CommunityId == communityId && ids.Contains(read.NotificationId))
            .Select(read => read.NotificationId)
            .ToListAsync(cancellationToken)).ToHashSet();
    }

    private void Add(NotificationAudience audience, long? communityId, NotificationMessage message)
    {
        var values = message.Values.Length == 0 ? null : string.Join(AppConsts.NotificationValueSeparator, message.Values.Select(value => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty));

        if (values is { Length: > AppConsts.NotificationValuesMaxLength })
            values = values[..AppConsts.NotificationValuesMaxLength];

        dbContext.Notifications.Add(new Notification
        {
            Audience = audience,
            CommunityId = communityId,
            Topic = message.Topic,
            Text = message.Text,
            Values = values,
            Url = message.Url is { Length: > AppConsts.NotificationUrlMaxLength } ? null : message.Url,
            CreatedAtUtc = clock.UtcNow
        });
    }

    private static NotificationItem Item(Notification notification, bool read)
    {
        return new NotificationItem(notification.Id, notification.Topic, notification.Text, notification.Values == null ? [] : notification.Values.Split(AppConsts.NotificationValueSeparator), notification.Url, notification.CreatedAtUtc, read);
    }
}
