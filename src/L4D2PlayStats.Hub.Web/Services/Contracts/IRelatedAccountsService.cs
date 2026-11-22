using L4D2PlayStats.Hub.Web.Models.Players;

namespace L4D2PlayStats.Hub.Web.Services.Contracts;

public interface IRelatedAccountsService
{
    Task<RelatedAccountsResult?> FindAsync(string? player, string? list, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<long>> ValidAsync(long communityId, IEnumerable<string>? relatedIds, CancellationToken cancellationToken = default);
}
