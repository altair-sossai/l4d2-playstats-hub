using L4D2PlayStats.Hub.Core.Modules.Players.Models;

namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PlayerSearchPageResult(PlayerDirectoryPage page, TimeZoneInfo timeZone)
{
    public List<PlayerSearchResult> Players { get; } = [.. page.Players.Select(player => new PlayerSearchResult(player, timeZone))];
    public int Total { get; } = page.Total;
    public int Page { get; } = page.Page;
    public bool HasMore { get; } = page.HasMore;
}
