using L4D2PlayStats.Hub.Core.Modules.Seasons;
using L4D2PlayStats.Hub.Core.Modules.Servers;

namespace L4D2PlayStats.Hub.Web.Models.Matches;

public class LastMatchesModel
{
    public IReadOnlyList<MatchCardModel> Matches { get; init; } = [];
    public IReadOnlyList<Season> Seasons { get; init; } = [];
    public IReadOnlyList<GameServer> Servers { get; init; } = [];
    public int? SeasonId { get; init; }
    public int? ServerId { get; init; }
    public required PaginationModel Pagination { get; init; }
}
