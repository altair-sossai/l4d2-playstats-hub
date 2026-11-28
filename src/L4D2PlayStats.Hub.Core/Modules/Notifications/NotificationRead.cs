namespace L4D2PlayStats.Hub.Core.Modules.Notifications;

public class NotificationRead
{
    public int NotificationId { get; set; }
    public long CommunityId { get; set; }
    public DateTime ReadAtUtc { get; set; }
}
