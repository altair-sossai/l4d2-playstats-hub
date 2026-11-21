using L4D2PlayStats.Hub.Core.Modules.Players.Models;

namespace L4D2PlayStats.Hub.Web.Models.Players;

public class ForcedNamesViewModel
{
    public required PagedListModel<ForcedNameEntry> Names { get; init; }
    public string? Search { get; init; }
    public string ReturnUrl { get; init; } = AppConsts.ForcedNamesUrl;
}
