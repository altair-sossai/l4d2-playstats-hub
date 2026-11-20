namespace L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;

public interface IPlayerMatchService
{
    Task<List<int>> RecentMatchIdsAsync(int? seasonId, int? year, IReadOnlyCollection<long> communityIds, int recentMatches, CancellationToken cancellationToken = default);
}
