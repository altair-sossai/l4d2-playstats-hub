using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;

namespace L4D2PlayStats.Hub.Tests.Support.Paging;

public class FakeRankingService(int count) : IRankingService
{
    private readonly IReadOnlyList<RankingPlayer> _players =
    [
        .. Enumerable.Range(0, count).Select(index => new RankingPlayer(PagingSamples.CommunityId(index))
        {
            Name = PagingSamples.Name(index),
            Wins = count - index,
            Position = index + 1
        })
    ];

    public Task<IReadOnlyList<RankingPlayer>> SeasonAsync(int seasonId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_players);
    }

    public Task<IReadOnlyList<RankingPlayer>> CurrentAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_players);
    }

    public Task<IReadOnlyList<RankingPlayer>> YearAsync(int year, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_players);
    }

    public Task<IReadOnlyList<RankingPlayer>> AllTimeAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_players);
    }

    public Task<IReadOnlyList<RankingPlayer>> MatchAsync(int matchId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<RankingPlayer>>([]);
    }
}