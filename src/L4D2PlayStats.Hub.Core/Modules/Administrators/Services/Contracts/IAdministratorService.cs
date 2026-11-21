using L4D2PlayStats.Hub.Core.Infrastructure.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Administrators.Services.Contracts;

public interface IAdministratorService
{
    Task<bool> IsAdministratorAsync(long communityId, CancellationToken cancellationToken = default);
    Task<List<Administrator>> ListAsync(CancellationToken cancellationToken = default);
    Task<Page<Administrator>> PageAsync(int page, int size, CancellationToken cancellationToken = default);
    Task AddAsync(long communityId, string? name, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(long communityId, CancellationToken cancellationToken = default);
}
