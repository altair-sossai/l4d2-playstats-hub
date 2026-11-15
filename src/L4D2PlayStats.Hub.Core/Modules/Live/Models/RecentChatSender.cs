namespace L4D2PlayStats.Hub.Core.Modules.Live.Models;

public record RecentChatSender(long CommunityId, string Name, string Text, DateTime When, bool Banned);
