using L4D2PlayStats.Hub.Core.Modules.Notifications.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Notifications.Models;

public record NotificationMessage(NotificationTopic Topic, string Text, string? Url, params object?[] Values);
