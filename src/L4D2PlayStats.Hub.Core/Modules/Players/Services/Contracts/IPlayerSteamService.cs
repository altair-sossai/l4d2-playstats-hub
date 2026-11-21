using L4D2PlayStats.Hub.Core.Modules.Players.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;

public interface IPlayerSteamService
{
    Task<List<long>> DueAsync(int count, CancellationToken cancellationToken = default);
    Task<bool> ApplyAsync(IReadOnlyCollection<PlayerSteamUpdate> updates, CancellationToken cancellationToken = default);
    Task<Dictionary<long, PlayerSteamDetails>> DetailsAsync(IReadOnlyCollection<long> communityIds, CancellationToken cancellationToken = default);
}
