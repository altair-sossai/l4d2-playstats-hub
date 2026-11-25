namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;

public interface IChampionshipTeamService
{
    Task<int> AddParticipantsAsync(int championshipId, IReadOnlyCollection<long> communityIds, long? addedBy, CancellationToken cancellationToken = default);
    Task RemoveParticipantAsync(int championshipId, long communityId, long? removedBy, CancellationToken cancellationToken = default);
    Task SetPotAsync(int championshipId, long communityId, int pot, CancellationToken cancellationToken = default);
    Task<ChampionshipTeam> CreateTeamAsync(int championshipId, string name, string? tag, long? createdBy, CancellationToken cancellationToken = default);
    Task<int> CreateTeamsAsync(int championshipId, int count, string prefix, long? createdBy, CancellationToken cancellationToken = default);
    Task UpdateTeamAsync(int teamId, string name, string? tag, int seed, long? updatedBy, CancellationToken cancellationToken = default);
    Task DeleteTeamAsync(int teamId, long? deletedBy, CancellationToken cancellationToken = default);
    Task AddMemberAsync(int teamId, long communityId, long? addedBy, CancellationToken cancellationToken = default);
    Task RemoveMemberAsync(int teamId, long communityId, long? removedBy, CancellationToken cancellationToken = default);
    Task SetLeadersAsync(int teamId, long? captain, long? viceCaptain, long? updatedBy, CancellationToken cancellationToken = default);
}
