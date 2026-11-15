using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Community;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Community.Support;

public sealed class CommunityWebSite : IDisposable
{
    public const string VotePage = "/community/vote";
    public const string HtmxHeader = "HX-Request";

    public HubWebFactory Factory { get; } = new();

    public long Voter => StatisticsBuilder.PlayerA1;

    public void Dispose()
    {
        Factory.Dispose();
    }

    public async Task StartAsync(CommunityRankingSettings settings, bool enabled = true, bool ranking = true)
    {
        await Factory.CompleteSetupAsync();
        await Factory.SaveAsync(new ModuleSettings { Ranking = ranking, CommunityRanking = enabled });
        await Factory.SaveAsync(settings);

        var credentials = await Factory.CreateServerAsync();

        using var server = Factory.ServerClient(credentials);

        var response = await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", CommunityTestHub.Night, 1, 300, 200));

        response.EnsureSuccessStatusCode();
    }

    public static CommunityPair? Pair(string html)
    {
        var left = Regex.Match(html, "name=\"Left\" value=\"(\\d+)\"");
        var right = Regex.Match(html, "name=\"Right\" value=\"(\\d+)\"");

        return left.Success && right.Success ? new CommunityPair(long.Parse(left.Groups[1].Value), long.Parse(right.Groups[1].Value)) : null;
    }

    public static Task<HttpResponseMessage> PostVoteAsync(HttpClient client, CommunityPair pair, string outcome)
    {
        return HubWebFactory.PostFormAsync(client, VotePage, VotePage, new Dictionary<string, string>
        {
            ["Left"] = pair.Left.ToString(),
            ["Right"] = pair.Right.ToString(),
            ["Outcome"] = outcome
        });
    }

    public Task<List<CommunityVote>> VotesAsync()
    {
        return Factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Set<CommunityVote>().AsNoTracking().OrderBy(v => v.Id).ToListAsync());
    }

    public Task<List<CommunityRating>> RatingsAsync()
    {
        return Factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Set<CommunityRating>().AsNoTracking().OrderBy(r => r.CommunityId).ToListAsync());
    }
}
