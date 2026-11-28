using L4D2PlayStats.Hub.Core.Modules.Notifications.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Notifications.Models;

public record NotificationItem(int Id, NotificationTopic Topic, string Text, string[] Values, string? Url, DateTime CreatedAtUtc, bool Read);
