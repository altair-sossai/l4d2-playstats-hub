using L4D2PlayStats.Hub.Core.Modules.Matches.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Matches.Services.Contracts;

public interface IMatchQueryService
{
    Task<MatchPage> ListAsync(MatchFilter filter, CancellationToken cancellationToken = default);
    Task<Match?> FindAsync(int id, CancellationToken cancellationToken = default);
    Task<Match?> LastAsync(CancellationToken cancellationToken = default);
}
