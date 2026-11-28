using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Notifications.Services.Contracts;

public interface INotificationService
{
    void Send(long communityId, NotificationMessage message);
    void Send(IEnumerable<long> communityIds, NotificationMessage message);
    void SendAdministrators(NotificationMessage message);
    void SendEveryone(NotificationMessage message);
    Task<Page<NotificationItem>> PageAsync(long communityId, bool administrator, int page, int size, CancellationToken cancellationToken = default);
    Task<List<NotificationItem>> LatestAsync(long communityId, bool administrator, int count, CancellationToken cancellationToken = default);
    Task<int> UnreadAsync(long communityId, bool administrator, CancellationToken cancellationToken = default);
    Task<NotificationItem?> OpenAsync(long communityId, bool administrator, int notificationId, CancellationToken cancellationToken = default);
    Task<int> ReadAllAsync(long communityId, bool administrator, CancellationToken cancellationToken = default);
    Task<int> CleanupAsync(CancellationToken cancellationToken = default);
}
