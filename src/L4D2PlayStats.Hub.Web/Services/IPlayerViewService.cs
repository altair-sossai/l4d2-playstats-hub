using L4D2PlayStats.Hub.Web.Models.Players;

namespace L4D2PlayStats.Hub.Web.Services;

public interface IPlayerViewService
{
    Task<PlayerPeriodModel?> PeriodAsync(int? seasonId, int? year, CancellationToken cancellationToken = default);
    Task<PlayersModel> ListAsync(PlayerPeriodModel period, PlayerResultProperty orderBy, bool asc, CancellationToken cancellationToken = default);
    Task<PlayerDetailsModel?> DetailsAsync(PlayerPeriodModel period, long communityId, long? compareWith, CancellationToken cancellationToken = default);
    Task<PlayerNotFoundModel> NotFoundAsync(PlayerPeriodModel period, long communityId, CancellationToken cancellationToken = default);
}
