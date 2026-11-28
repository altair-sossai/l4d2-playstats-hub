namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;

public interface IChampionshipMatchService
{
    Task SyncAsync(IReadOnlyCollection<int> matchIds, CancellationToken cancellationToken = default);
    Task<int> CloseIdleAsync(CancellationToken cancellationToken = default);
    Task CloseAsync(int gameId, long? by, CancellationToken cancellationToken = default);
}
