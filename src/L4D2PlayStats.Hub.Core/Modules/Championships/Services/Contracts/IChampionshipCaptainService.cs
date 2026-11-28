using L4D2PlayStats.Hub.Core.Modules.Championships.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;

public interface IChampionshipCaptainService
{
    Task<List<ChampionshipCaptainTeam>> TeamsAsync(long communityId, CancellationToken cancellationToken = default);
    Task SetViceAsync(int teamId, long captain, long? viceCaptain, CancellationToken cancellationToken = default);
}
