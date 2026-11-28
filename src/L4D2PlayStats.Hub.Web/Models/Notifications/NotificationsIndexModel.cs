namespace L4D2PlayStats.Hub.Web.Models.Notifications;

public record NotificationsIndexModel(PagedListModel<NotificationView> Notifications, int Unread, TimeZoneInfo TimeZone);
