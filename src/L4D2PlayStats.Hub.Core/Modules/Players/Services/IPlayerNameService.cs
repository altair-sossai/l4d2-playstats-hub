namespace L4D2PlayStats.Hub.Core.Modules.Players.Services;

public interface IPlayerNameService
{
    Task<string?> KnownNameAsync(long communityId, CancellationToken cancellationToken = default);
}
