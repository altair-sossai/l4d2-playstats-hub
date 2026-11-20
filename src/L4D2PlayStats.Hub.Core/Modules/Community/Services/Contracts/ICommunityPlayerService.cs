namespace L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;

public interface ICommunityPlayerService
{
    Task<int> MatchesAsync(long communityId, CancellationToken cancellationToken = default);
    Task<HashSet<long>> CandidatesAsync(int minimumMatches, DateTime? playedSince = null, CancellationToken cancellationToken = default);
    Task<HashSet<long>> FamiliarAsync(long communityId, CancellationToken cancellationToken = default);
    Task<Dictionary<long, string>> NamesAsync(IReadOnlyCollection<long> communityIds, CancellationToken cancellationToken = default);
    Task<Dictionary<long, List<string>>> MostUsedNamesAsync(IReadOnlyCollection<long> communityIds, int count, CancellationToken cancellationToken = default);
}