namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;

public interface IChampionshipJoinService
{
    bool IsOpen(Championship championship);
    Task<ChampionshipJoinRequest?> FindAsync(int championshipId, long communityId, CancellationToken cancellationToken = default);
    Task<List<ChampionshipJoinRequest>> ListAsync(int championshipId, CancellationToken cancellationToken = default);
    Task<ChampionshipJoinRequest> AskAsync(int championshipId, long communityId, string? message, CancellationToken cancellationToken = default);
    Task CancelAsync(int championshipId, long communityId, CancellationToken cancellationToken = default);
    Task ApproveAsync(int requestId, long? answeredBy, CancellationToken cancellationToken = default);
    Task RefuseAsync(int requestId, long? answeredBy, CancellationToken cancellationToken = default);
}
