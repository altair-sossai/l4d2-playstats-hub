using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;

public interface IChampionshipService
{
    Task<List<ChampionshipSummary>> ActiveAsync(CancellationToken cancellationToken = default);
    Task<Page<ChampionshipSummary>> HistoryAsync(int page, int size, CancellationToken cancellationToken = default);
    Task<Page<ChampionshipSummary>> PageAsync(int page, int size, CancellationToken cancellationToken = default);
    Task<Championship> CreateAsync(string name, IReadOnlyList<ChampionshipStageTemplate> stages, long? createdBy, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, ChampionshipSettingsForm form, long? updatedBy, CancellationToken cancellationToken = default);
    Task UpdateRulesAsync(int id, string? english, string? portuguese, string? spanish, long? updatedBy, CancellationToken cancellationToken = default);
    Task SetStatusAsync(int id, ChampionshipStatus status, string? reason, long? updatedBy, CancellationToken cancellationToken = default);
    Task<ChampionshipImageSet?> ImagesAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> IsPublicAsync(int id, CancellationToken cancellationToken = default);
    Task<ChampionshipImageSet> SetImagesAsync(int id, ChampionshipImageSet images, long? updatedBy, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
