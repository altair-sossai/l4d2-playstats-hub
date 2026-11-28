namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;

public interface IChampionshipScheduleService
{
    Task<ChampionshipDateProposal> ProposeAsync(int seriesId, DateTime dateUtc, long communityId, CancellationToken cancellationToken = default);
    Task AnswerAsync(int proposalId, bool accept, long communityId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int proposalId, long communityId, bool administrator, CancellationToken cancellationToken = default);
}
