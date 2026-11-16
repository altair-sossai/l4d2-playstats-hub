namespace L4D2PlayStats.Hub.Web.Models.Live;

public class ChatBansModel(PagedListModel<ChatBanResult> bans, string? query, int totalBans, TimeZoneInfo timeZone)
{
    public PagedListModel<ChatBanResult> Bans { get; } = bans;
    public string? Query { get; } = query;
    public int TotalBans { get; } = totalBans;
    public TimeZoneInfo TimeZone { get; } = timeZone;
}
