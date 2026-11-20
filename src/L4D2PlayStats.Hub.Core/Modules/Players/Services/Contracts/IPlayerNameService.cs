namespace L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;

public interface IPlayerNameService
{
    Task<string?> KnownNameAsync(long communityId, CancellationToken cancellationToken = default);
}
