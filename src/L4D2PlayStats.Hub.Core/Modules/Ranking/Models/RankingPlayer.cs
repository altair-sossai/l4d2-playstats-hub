using L4D2PlayStats.Hub.Core.Modules.Matches;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Ranking.Models;

public class RankingPlayer(long communityId) : MatchPlayerStatistics
{
    public long CommunityId { get; } = communityId;
    public string SteamId => SteamIds.SteamId(CommunityId);
    public string Steam3 => SteamIds.Steam3(CommunityId);
    public string ProfileUrl => SteamIds.ProfileUrl(CommunityId);
    public int Position { get; set; }

    public string? Name
    {
        get;
        set
        {
            field = value;

            if (string.IsNullOrEmpty(value))
                return;

            var previousName = PreviousNames.FirstOrDefault(pn => pn.Name.Equals(value, StringComparison.CurrentCultureIgnoreCase));
            if (previousName == null)
            {
                previousName = new PreviousName(value);
                PreviousNames.Add(previousName);
            }

            previousName.TimesUsed++;

            var ordered = PreviousNames.OrderByDescending(pn => pn.TimesUsed).ToList();
            PreviousNames.Clear();
            PreviousNames.AddRange(ordered);
        }
    }

    public string? ForcedName { get; set; }
    public string MainName => ForcedName ?? PreviousNames.FirstOrDefault()?.Name ?? Name ?? AppConsts.UnknownPlayerName;
    public string MostUsedNames => string.Join(" | ", PreviousNames.Take(AppConsts.MostUsedNames).Select(pn => pn.Name));
    public List<PreviousName> PreviousNames { get; } = [];
    public Dictionary<long, PlayerRelation> Relations { get; } = [];
    public Dictionary<string, PlayerCampaign> Campaigns { get; } = [];

    public decimal Experience
    {
        get;
        set => field = Math.Max(0, value);
    }

    public decimal? PreviousExperience
    {
        get;
        set => field = value == null ? null : Math.Max(0, value.Value);
    }

    public decimal? ExperienceDifference => PreviousExperience == null ? null : Experience - PreviousExperience;
    public int Wins { get; set; }
    public int Loss { get; set; }
    public int Draws { get; set; }
    public int RageQuit { get; set; }
    public List<MatchOutcome> Results { get; } = [];
    public int MaxWinStreak => MaxStreak(MatchOutcome.Win);
    public int MaxLossStreak => MaxStreak(MatchOutcome.Loss);
    public int MaxDrawStreak => MaxStreak(MatchOutcome.Draw);
    public int Punishment { get; set; }
    public int Games => Wins + Loss;
    public decimal WinRate => Games == 0 ? 0 : Wins / (decimal)Games;
    public int Stars => MvpSiDamage + MvpCommon;

    public PlayerRelation Relation(long otherCommunityId)
    {
        if (Relations.TryGetValue(otherCommunityId, out var relation))
            return relation;

        relation = new PlayerRelation(otherCommunityId);
        Relations.Add(otherCommunityId, relation);

        return relation;
    }

    public PlayerCampaign Campaign(string? name)
    {
        var key = name ?? string.Empty;

        if (Campaigns.TryGetValue(key, out var campaign))
            return campaign;

        campaign = new PlayerCampaign();
        Campaigns.Add(key, campaign);

        return campaign;
    }

    private int MaxStreak(MatchOutcome outcome)
    {
        var max = 0;
        var length = 0;

        foreach (var current in Results)
        {
            length = current == outcome ? length + 1 : 0;
            max = Math.Max(max, length);
        }

        return max;
    }
}
