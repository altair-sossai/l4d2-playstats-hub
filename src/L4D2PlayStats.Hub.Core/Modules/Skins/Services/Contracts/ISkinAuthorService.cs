using L4D2PlayStats.Hub.Core.Modules.Skins.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;

public interface ISkinAuthorService
{
    Task<IReadOnlyList<long>> DueAsync(DateTime syncedBeforeUtc, int count, CancellationToken cancellationToken = default);
    Task<int> ApplyAsync(IReadOnlyCollection<long> checkedIds, IReadOnlyDictionary<long, string> names, CancellationToken cancellationToken = default);
    Task<SkinAuthorStatus> StatusAsync(CancellationToken cancellationToken = default);
}
