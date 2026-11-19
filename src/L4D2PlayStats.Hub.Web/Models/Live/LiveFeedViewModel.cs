using L4D2PlayStats.Hub.Core.Modules.Live.Models;

namespace L4D2PlayStats.Hub.Web.Models.Live;

public class LiveFeedViewModel
{
    public string PublicId { get; init; } = null!;
    public IReadOnlyList<FeedItem> Items { get; init; } = [];
    public TimeZoneInfo TimeZone { get; init; } = TimeZoneInfo.Utc;
}
