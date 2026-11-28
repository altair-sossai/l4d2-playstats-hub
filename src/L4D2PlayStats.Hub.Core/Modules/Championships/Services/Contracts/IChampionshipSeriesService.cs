using L4D2PlayStats.Hub.Core.Modules.Championships.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;

public interface IChampionshipSeriesService
{
    Task<List<ChampionshipCandidateMatch>> CandidatesAsync(int seriesId, CancellationToken cancellationToken = default);
    Task LinkMatchAsync(int gameId, int matchId, long? by, CancellationToken cancellationToken = default);
    Task SetManualResultAsync(int gameId, int teamAScore, int teamBScore, DateTime? playedAtUtc, long? by, CancellationToken cancellationToken = default);
    Task SetPenaltiesAsync(int gameId, int teamAPenalty, int teamBPenalty, string? notes, long? by, CancellationToken cancellationToken = default);
    Task ClearResultAsync(int gameId, long? by, CancellationToken cancellationToken = default);
    Task SetCampaignAsync(int gameId, int? campaignId, long? by, CancellationToken cancellationToken = default);
    Task PickAsync(int gameId, int campaignId, long communityId, CancellationToken cancellationToken = default);
    Task VetoAsync(int seriesId, int campaignId, long communityId, CancellationToken cancellationToken = default);
    Task SetVetoAsync(int seriesId, bool teamA, int? campaignId, long? by, CancellationToken cancellationToken = default);
    Task WalkoverAsync(int seriesId, int? winnerTeamId, long? by, CancellationToken cancellationToken = default);
    Task SetScheduleAsync(int seriesId, DateTime? scheduledAtUtc, DateTime? deadlineUtc, string? notes, long? by, CancellationToken cancellationToken = default);
}
