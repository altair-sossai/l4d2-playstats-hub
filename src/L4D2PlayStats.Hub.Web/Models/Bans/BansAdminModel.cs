using L4D2PlayStats.Hub.Core.Modules.Bans;
using L4D2PlayStats.Hub.Core.Modules.Bans.Models;

namespace L4D2PlayStats.Hub.Web.Models.Bans;

public class BansAdminModel
{
    public required PagedListModel<Ban> Active { get; init; }
    public required PagedListModel<Ban> Ended { get; init; }
    public required PagedListModel<BanAlertEntry> Alerts { get; init; }
    public required int ActiveCount { get; init; }
    public required string Tab { get; init; }
    public required string ReturnUrl { get; init; }
    public required string AlertsReturnUrl { get; init; }
    public required TimeZoneInfo TimeZone { get; init; }
    public string? Search { get; init; }
}
