using L4D2PlayStats.Hub.Core.Modules.Players.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Services;

public interface IPlayerMatchService
{
    Task<List<PlayerMatch>> LastAsync(int seasonId, IReadOnlyCollection<long> communityIds, int recentMatches, CancellationToken cancellationToken = default);
    Task<List<PlayerMatch>> LastInYearAsync(int year, IReadOnlyCollection<long> communityIds, int recentMatches, CancellationToken cancellationToken = default);
}
