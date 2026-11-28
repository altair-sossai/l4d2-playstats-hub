using L4D2PlayStats.Hub.Core.Modules.Notifications.Enums;

namespace L4D2PlayStats.Hub.Web.Models.Notifications;

public record NotificationView(int Id, NotificationTopic Topic, string Text, string? Url, DateTime CreatedAtUtc, bool Read);
