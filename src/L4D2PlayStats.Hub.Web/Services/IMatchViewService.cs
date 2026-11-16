using L4D2PlayStats.Hub.Core.Modules.Matches.Models;
using L4D2PlayStats.Hub.Web.Models.Matches;

namespace L4D2PlayStats.Hub.Web.Services;

public interface IMatchViewService
{
    Task<LastMatchesModel> ListAsync(int? seasonId, int? serverId, int page, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MatchCardModel>> CardsAsync(MatchFilter filter, CancellationToken cancellationToken = default);
    Task<MatchDetailsModel?> DetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<StatisticsDetailsModel?> StatisticsAsync(int id, int fileId, CancellationToken cancellationToken = default);
}
