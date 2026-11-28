namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;

public interface IChampionshipIdentityService
{
    Task<List<string>> ProposeAsync(int teamId, long leader, string? name, string? tag, string? flag, CancellationToken cancellationToken = default);
    Task<List<string>> CancelAsync(int teamId, long leader, CancellationToken cancellationToken = default);
    Task<List<string>> RemoveFlagAsync(int teamId, long? by, bool leader, CancellationToken cancellationToken = default);
    Task<List<string>> SetFlagAsync(int teamId, string flag, long? by, CancellationToken cancellationToken = default);
    Task<List<string>> ApproveAsync(int teamId, long? by, CancellationToken cancellationToken = default);
    Task<List<string>> RejectAsync(int teamId, string? reason, long? by, CancellationToken cancellationToken = default);
    Task<List<string>> FilesAsync(int championshipId, int? teamId = null, CancellationToken cancellationToken = default);
}
