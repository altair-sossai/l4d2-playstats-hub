namespace L4D2PlayStats.Hub.Core.Modules.Administrators.Services;

public interface ISetupService
{
    Task<bool> IsPendingAsync(CancellationToken cancellationToken = default);
    Task<string?> EnsureTokenAsync(CancellationToken cancellationToken = default);
    Task<bool> IsValidTokenAsync(string? token, CancellationToken cancellationToken = default);
    Task<bool> ClaimAsync(string? token, long communityId, string? name, CancellationToken cancellationToken = default);
    Task CompleteAsync(CancellationToken cancellationToken = default);
}
