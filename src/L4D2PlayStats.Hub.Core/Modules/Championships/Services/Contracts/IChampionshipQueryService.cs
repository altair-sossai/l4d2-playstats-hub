using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Players;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;

public interface IChampionshipQueryService
{
    Task<Dictionary<long, PlayerProfile>> ProfilesAsync(Championship championship, CancellationToken cancellationToken = default);
    Task<Dictionary<long, string>> NamesAsync(IReadOnlyCollection<long> communityIds, CancellationToken cancellationToken = default);
    Task<List<ChampionshipPlayerStatistics>> StatisticsAsync(Championship championship, CancellationToken cancellationToken = default);
    Task<ChampionshipMatchLink?> MatchLinkAsync(int matchId, CancellationToken cancellationToken = default);
    Task<ChampionshipTeamFlag?> FlagAsync(int teamId, CancellationToken cancellationToken = default);
}
