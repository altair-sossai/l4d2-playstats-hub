using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;

public interface IChampionshipStageService
{
    Task<ChampionshipStage> AddAsync(int championshipId, string name, ChampionshipStageType type, long? by, CancellationToken cancellationToken = default);
    Task UpdateAsync(int stageId, ChampionshipStageForm form, long? by, CancellationToken cancellationToken = default);
    Task DeleteAsync(int stageId, long? by, CancellationToken cancellationToken = default);
    Task<List<int>> EntrantsAsync(int stageId, CancellationToken cancellationToken = default);
    Task GenerateAsync(int stageId, long? by, CancellationToken cancellationToken = default);
    Task ResetAsync(int stageId, long? by, CancellationToken cancellationToken = default);
    Task DrawCampaignsAsync(int stageId, long? by, CancellationToken cancellationToken = default);
    Task ClearCampaignsAsync(int stageId, long? by, CancellationToken cancellationToken = default);
    Task SetRoundDeadlineAsync(int stageId, int round, DateTime? deadlineUtc, long? by, CancellationToken cancellationToken = default);
    Task AddPenaltyAsync(int stageId, int teamId, int points, string? reason, long? by, CancellationToken cancellationToken = default);
    Task RemovePenaltyAsync(int penaltyId, long? by, CancellationToken cancellationToken = default);
}
