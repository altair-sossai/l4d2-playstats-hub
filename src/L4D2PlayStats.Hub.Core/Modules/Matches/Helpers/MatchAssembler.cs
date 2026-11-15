using L4D2PlayStats.Hub.Core.Modules.Campaigns.Models;
using L4D2PlayStats.Hub.Core.Modules.Matches.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Matches.Helpers;

public static class MatchAssembler
{
    public static List<MatchDraft> Assemble(IEnumerable<AssemblyFile> files, CampaignCatalog catalog, bool includeUnknownCampaigns)
    {
        var drafts = new List<MatchDraft>();

        var groups = files
            .GroupBy(f => (f.File.ConfigurationName, f.File.TeamSize))
            .Select(group => group.OrderByDescending(f => f.File.FileDate).ThenByDescending(f => f.File.Id).ToList());

        foreach (var group in groups)
            drafts.AddRange(AssembleGroup(group, catalog, includeUnknownCampaigns));

        return [.. drafts.OrderByDescending(d => d.Newest.File.FileDate).ThenByDescending(d => d.Newest.File.Id)];
    }

    private static List<MatchDraft> AssembleGroup(List<AssemblyFile> files, CampaignCatalog catalog, bool includeUnknownCampaigns)
    {
        var drafts = new List<MatchDraft>();
        MatchDraft? draft = null;
        AssemblyFile? last = null;
        string? lastMap = null;

        foreach (var file in files)
        {
            var statistics = file.Statistics;
            if (statistics == null)
                continue;

            var gameRound = statistics.GameRound;
            var mapName = gameRound?.MapName;

            if (lastMap == mapName)
                continue;

            var teamA = statistics.Scoring?.TeamA;
            var teamB = statistics.Scoring?.TeamB;
            var halfA = statistics.Halves.FirstOrDefault(half => half.RoundHalf?.Team == 'A');
            var halfB = statistics.Halves.FirstOrDefault(half => half.RoundHalf?.Team == 'B');

            if (gameRound == null || mapName == null || teamA == null || teamB == null || halfA == null || halfB == null)
                continue;

            var knownMap = catalog.IsKnownMap(mapName);
            if (!knownMap && !includeUnknownCampaigns)
                continue;

            if (teamA.Score == 0 && teamB.Score == 0)
                continue;

            var sequential = false;
            string? campaignName = null;

            if (knownMap)
            {
                var campaign = catalog.FindByName(draft?.Campaign) ?? catalog.FindUsingMapName(mapName);
                if (campaign == null)
                    continue;

                if (!string.IsNullOrEmpty(lastMap) && !campaign.SequentialMaps(mapName, lastMap))
                    campaign = catalog.FindUsingMapName(mapName);

                if (campaign == null)
                    continue;

                sequential = draft != null && !string.IsNullOrEmpty(lastMap) && campaign.SequentialMaps(mapName, lastMap);
                campaignName = campaign.Name;
            }
            else
            {
                sequential = draft is { Campaign: null } && last != null && SequentialUnknownMaps(file, last);
            }

            if (!sequential)
            {
                draft = new MatchDraft
                {
                    Campaign = campaignName,
                    ConfigurationName = file.File.ConfigurationName,
                    TeamSize = gameRound.TeamSize
                };

                drafts.Add(draft);
            }

            draft!.Files.Add(file);
            last = file;
            lastMap = mapName;
        }

        return drafts;
    }

    private static bool SequentialUnknownMaps(AssemblyFile older, AssemblyFile newer)
    {
        var gap = newer.File.FileDate - older.File.FileDate;

        return newer.File.Round == older.File.Round + 1
               && gap >= TimeSpan.Zero
               && gap <= TimeSpan.FromHours(AppConsts.UnknownCampaignMapGapInHours);
    }
}
