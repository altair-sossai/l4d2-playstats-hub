using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Enums;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;

public interface IForcedNameService
{
    Task<Page<ForcedNameEntry>> PageAsync(PlayerSearch search, int page, int size, CancellationToken cancellationToken = default);
    Task<ForcedNameSaveResult> SaveAsync(long communityId, string? name, long? updatedBy, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long communityId, CancellationToken cancellationToken = default);
    Task<ForcedNameImport> ImportAsync(string? text, long? updatedBy, CancellationToken cancellationToken = default);
    Task LoadAsync(CancellationToken cancellationToken = default);
}
