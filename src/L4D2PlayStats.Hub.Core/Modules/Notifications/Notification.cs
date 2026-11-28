using L4D2PlayStats.Hub.Core.Modules.Notifications.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Notifications;

public class Notification
{
    public int Id { get; set; }
    public NotificationAudience Audience { get; set; }
    public long? CommunityId { get; set; }
    public NotificationTopic Topic { get; set; }
    public string Text { get; set; } = null!;
    public string? Values { get; set; }
    public string? Url { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
