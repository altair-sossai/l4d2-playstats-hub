namespace L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;

public interface IPlayerNameService
{
    Task<string?> KnownNameAsync(long communityId, CancellationToken cancellationToken = default);
    Task<string?> RealNameAsync(long communityId, CancellationToken cancellationToken = default);
    Task<Dictionary<long, string>> NamesAsync(IReadOnlyCollection<long> communityIds, CancellationToken cancellationToken = default);
    Task<Dictionary<long, string>> DisplayNamesAsync(CancellationToken cancellationToken = default);
}
