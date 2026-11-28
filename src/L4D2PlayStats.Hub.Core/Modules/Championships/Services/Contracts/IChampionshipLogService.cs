using L4D2PlayStats.Hub.Core.Infrastructure.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;

public interface IChampionshipLogService
{
    void Add(int championshipId, long? communityId, string text, params object?[] values);
    Task<Page<ChampionshipLog>> PageAsync(int championshipId, int page, int size, CancellationToken cancellationToken = default);
}
