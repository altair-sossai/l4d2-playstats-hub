using L4D2PlayStats.Hub.Core.Modules.Championships;
using L4D2PlayStats.Hub.Web.Helpers;

namespace L4D2PlayStats.Hub.Web.Models.Championships;

public class ChampionshipAdminModel
{
    public required ChampionshipPageModel Page { get; init; }
    public required PagedListModel<ChampionshipJoinRequest> Requests { get; init; }
    public int PendingRequests { get; init; }
    public required PagedListModel<ChampionshipParticipant> Participants { get; init; }
    public required PagedListModel<ChampionshipTeam> Teams { get; init; }
    public required PagedListModel<long> Free { get; init; }
    public required PagedListModel<ChampionshipSeries> Series { get; init; }
    public required PagedListModel<ChampionshipLog> History { get; init; }
    public required Dictionary<int, List<int>> Entrants { get; init; }
    public int? StageFilter { get; init; }

    public Championship Championship => Page.Championship;

    public string Url => ChampionshipUrls.Admin(Championship.Id);
}
