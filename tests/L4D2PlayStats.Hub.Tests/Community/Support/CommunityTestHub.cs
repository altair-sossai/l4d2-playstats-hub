using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Community;
using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Services;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Community.Support;

public sealed class CommunityTestHub : IDisposable
{
    public static readonly DateTime Night = new(2026, 11, 14, 21, 0, 0);

    private GameServer? _server;

    public TestHub Hub { get; } = new();

    public List<long> TeamA { get; } = StatisticsBuilder.Sample().Survivors('A');

    public List<long> TeamB { get; } = StatisticsBuilder.Sample().Survivors('B');

    public List<long> Everyone => [.. TeamA, .. TeamB];

    public long Voter => TeamA[0];

    public void Dispose()
    {
        Hub.Dispose();
    }

    public async Task PlayAsync(DateTime when, params long[] absent)
    {
        _server ??= (await Hub.CreateServerAsync()).Server;

        var builder = StatisticsBuilder.Map("c2m1_highway", when, 1, 300, 200);

        foreach (var communityId in absent)
            builder.WithoutPlayer(communityId);

        await Hub.UploadAsync(_server, builder);
    }

    public Task SaveAsync<T>(T settings) where T : class, new()
    {
        return Hub.RunAsync(services => services.GetRequiredService<ISettingsService>().SaveAsync(settings));
    }

    public async Task StartAsync(CommunityRankingSettings settings)
    {
        await PlayAsync(Night);
        await SaveAsync(settings);
    }

    public Task<CommunityBallot> BallotAsync(long voterId)
    {
        return Hub.RunAsync(services => services.GetRequiredService<ICommunityVoteService>().BallotAsync(voterId));
    }

    public Task<CommunityVoteResult> VoteAsync(long voterId, long leftPlayerId, long rightPlayerId, CommunityVoteOutcome outcome)
    {
        return Hub.RunAsync(services => services.GetRequiredService<ICommunityVoteService>().VoteAsync(voterId, leftPlayerId, rightPlayerId, outcome));
    }

    public async Task<CommunityPair?> AnswerAsync(long voterId, CommunityVoteOutcome outcome)
    {
        var ballot = await BallotAsync(voterId);

        if (ballot.Status != CommunityBallotStatus.Ready)
            return null;

        var pair = new CommunityPair(ballot.Left!.CommunityId, ballot.Right!.CommunityId);
        var result = await VoteAsync(voterId, pair.Left, pair.Right, outcome);

        Assert.AreEqual(CommunityVoteResult.Accepted, result);

        return pair;
    }

    public async Task<List<CommunityPair>> AnswerEverythingAsync(long voterId, CommunityVoteOutcome outcome)
    {
        var pairs = new List<CommunityPair>();

        while (await AnswerAsync(voterId, outcome) is { } pair)
            pairs.Add(pair);

        return pairs;
    }

    public Task<List<CommunityRating>> RatingsAsync()
    {
        return Hub.RunAsync(services => services.GetRequiredService<HubDbContext>().Set<CommunityRating>().AsNoTracking().OrderBy(r => r.CommunityId).ToListAsync());
    }

    public Task<List<CommunityVote>> VotesAsync()
    {
        return Hub.RunAsync(services => services.GetRequiredService<HubDbContext>().Set<CommunityVote>().AsNoTracking().OrderBy(v => v.Id).ToListAsync());
    }

    public Task<IReadOnlyList<CommunityRankingEntry>> RankingAsync()
    {
        return Hub.RunAsync(services => services.GetRequiredService<ICommunityRankingService>().ListAsync());
    }

    public static string Snapshot(IEnumerable<CommunityRating> ratings)
    {
        return string.Join('|', ratings.Select(r => $"{r.CommunityId}:{r.Rating:R}:{r.Comparisons}:{r.Wins}:{r.Losses}:{r.Draws}"));
    }
}
