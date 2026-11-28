using L4D2PlayStats.Hub.Web.Models.Notifications;

namespace L4D2PlayStats.Hub.Web.Services.Contracts;

public interface INotificationViewService
{
    Task<NotificationBellModel?> BellAsync(CancellationToken cancellationToken = default);
    Task<NotificationsIndexModel?> PageAsync(int page, CancellationToken cancellationToken = default);
}
