using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;

namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PlayerSearchPageResult(Page<PlayerDirectoryEntry> page, TimeZoneInfo timeZone)
{
    public List<PlayerSearchResult> Players { get; } = [.. page.Items.Select(player => new PlayerSearchResult(player, timeZone))];
    public int Total { get; } = page.Total;
    public int Page { get; } = page.Number;
    public bool HasMore { get; } = page.HasMore;
}
