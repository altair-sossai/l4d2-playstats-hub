namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;

public interface IChampionshipDraftService
{
    Task DrawOrderAsync(int championshipId, long? by, CancellationToken cancellationToken = default);
    Task<ChampionshipDraftPick> PickAsync(int championshipId, long communityId, long? by, CancellationToken cancellationToken = default);
    Task<ChampionshipDraftPick> SpinAsync(int championshipId, long? by, CancellationToken cancellationToken = default);
    Task<int> DrawPotsAsync(int championshipId, long? by, CancellationToken cancellationToken = default);
    Task UndoAsync(int championshipId, long? by, CancellationToken cancellationToken = default);
    Task ResetAsync(int championshipId, long? by, CancellationToken cancellationToken = default);
}
