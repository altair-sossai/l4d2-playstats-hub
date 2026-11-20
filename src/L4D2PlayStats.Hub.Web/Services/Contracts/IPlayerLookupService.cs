using L4D2PlayStats.Hub.Web.Models.Players;

namespace L4D2PlayStats.Hub.Web.Services.Contracts;

public interface IPlayerLookupService
{
    Task<PlayerLookupResult?> LookupAsync(string? value, CancellationToken cancellationToken = default);
}
