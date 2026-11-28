namespace L4D2PlayStats.Hub.Web.Models.Notifications;

public record NotificationBellModel(List<NotificationView> Latest, int Unread, TimeZoneInfo TimeZone);
