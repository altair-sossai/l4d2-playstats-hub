using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Campaigns.Services;
using L4D2PlayStats.Hub.Core.Modules.GameModes.Services;
using L4D2PlayStats.Hub.Core.Modules.Matches.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Matches.Models;
using L4D2PlayStats.Hub.Core.Modules.Players;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles;
using Microsoft.EntityFrameworkCore;
using ParsedStatistics = L4D2PlayStats.Statistics;

namespace L4D2PlayStats.Hub.Core.Modules.Matches.Services;

public class MatchBuildService(
    HubDbContext dbContext,
    HubState hubState,
    ICampaignService campaignService,
    IGameModeService gameModeService,
    ISettingsService settingsService,
    IClock clock) : IMatchBuildService
{
    public async Task RebuildAroundAsync(int gameServerId, int seasonId, DateTime fileDate, CancellationToken cancellationToken = default)
    {
        var dates = await dbContext.StatisticsFiles
            .Where(f => f.GameServerId == gameServerId && f.SeasonId == seasonId)
            .OrderBy(f => f.FileDate)
            .Select(f => f.FileDate)
            .ToListAsync(cancellationToken);

        var cluster = FileClusters.Around(dates, fileDate);
        if (cluster == null)
            return;

        var files = await dbContext.StatisticsFiles
            .Where(f => f.GameServerId == gameServerId && f.SeasonId == seasonId && f.FileDate >= cluster.Value.Start && f.FileDate <= cluster.Value.End)
            .ToListAsync(cancellationToken);

        await RebuildAsync(gameServerId, seasonId, files, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        hubState.DataChanged();
    }

    public async Task<int> RebuildAllAsync(CancellationToken cancellationToken = default)
    {
        var scopes = await dbContext.StatisticsFiles
            .Select(f => new { f.GameServerId, f.SeasonId })
            .Distinct()
            .ToListAsync(cancellationToken);

        var matches = 0;

        foreach (var scope in scopes)
        {
            var files = await dbContext.StatisticsFiles
                .Where(f => f.GameServerId == scope.GameServerId && f.SeasonId == scope.SeasonId)
                .OrderBy(f => f.FileDate)
                .ToListAsync(cancellationToken);

            foreach (var cluster in FileClusters.Split(files))
                matches += await RebuildAsync(scope.GameServerId, scope.SeasonId, cluster, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
        }

        hubState.DataChanged();

        return matches;
    }

    private async Task<int> RebuildAsync(int gameServerId, int seasonId, List<StatisticsFile> files, CancellationToken cancellationToken)
    {
        var catalog = await campaignService.GetCatalogAsync(cancellationToken);
        var ruleSet = await gameModeService.GetRuleSetAsync(cancellationToken);
        var settings = await settingsService.GetAsync<RankingSettings>(cancellationToken);

        var matchIds = files.Where(f => f.MatchId != null).Select(f => f.MatchId!.Value).Distinct().ToList();
        if (matchIds.Count > 0)
        {
            var existing = await dbContext.Matches.Where(m => matchIds.Contains(m.Id)).ToListAsync(cancellationToken);
            dbContext.Matches.RemoveRange(existing);
        }

        foreach (var file in files)
        {
            file.MatchId = null;
            file.Match = null;
            file.MatchOrder = null;
        }

        var assemblyFiles = files.Select(file => new AssemblyFile(file, Parse(file.Content)));
        var drafts = MatchAssembler.Assemble(assemblyFiles, catalog, settings.IncludeUnknownCampaigns);
        var matches = new List<Match>();

        foreach (var draft in drafts)
        {
            var match = MatchFactory.Create(draft, gameServerId, seasonId, ruleSet);
            var order = draft.Files.Count;

            foreach (var draftFile in draft.Files)
            {
                order--;
                draftFile.File.Match = match;
                draftFile.File.MatchOrder = order;
            }

            dbContext.Matches.Add(match);
            matches.Add(match);
        }

        await UpdateProfilesAsync(matches, cancellationToken);

        return matches.Count;
    }

    private async Task UpdateProfilesAsync(List<Match> matches, CancellationToken cancellationToken)
    {
        var seen = new Dictionary<long, (string? Name, DateTime When)>();

        foreach (var match in matches.OrderBy(m => m.LastFileDate))
            foreach (var player in match.Players.Where(p => SteamIds.IsValid(p.CommunityId)))
            {
                var when = match.EndedAtUtc ?? clock.UtcNow;
                var name = string.IsNullOrEmpty(player.Name) && seen.TryGetValue(player.CommunityId, out var previous) ? previous.Name : player.Name;

                seen[player.CommunityId] = (name, when);
            }

        if (seen.Count == 0)
            return;

        var ids = seen.Keys.ToList();
        var profiles = await dbContext.PlayerProfiles.Where(p => ids.Contains(p.CommunityId)).ToDictionaryAsync(p => p.CommunityId, cancellationToken);

        foreach (var local in dbContext.PlayerProfiles.Local.Where(p => seen.ContainsKey(p.CommunityId)))
            profiles.TryAdd(local.CommunityId, local);

        foreach (var (communityId, (name, when)) in seen)
        {
            if (!profiles.TryGetValue(communityId, out var profile))
            {
                profile = new PlayerProfile { CommunityId = communityId, FirstSeenAtUtc = when, LastSeenAtUtc = when };
                dbContext.PlayerProfiles.Add(profile);
            }

            if (when < profile.FirstSeenAtUtc)
                profile.FirstSeenAtUtc = when;

            if (when < profile.LastSeenAtUtc)
                continue;

            profile.LastSeenAtUtc = when;

            if (!string.IsNullOrEmpty(name))
                profile.Name = name;
        }
    }

    private static ParsedStatistics? Parse(string content)
    {
        return ParsedStatistics.TryParse(content, out var statistics) ? statistics : null;
    }
}
