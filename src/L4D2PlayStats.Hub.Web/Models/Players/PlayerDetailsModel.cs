using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Helpers;
using L4D2PlayStats.Hub.Web.Models.Matches;
using L4D2PlayStats.Hub.Web.Models.Ranking;

namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PlayerDetailsModel(
    RankingModel firstPlayerRanking,
    RankingModel? secondPlayerRanking,
    IReadOnlyList<RankingPlayer> players,
    IReadOnlyList<MatchCardModel> matches,
    IReadOnlyList<PlayerConnectionResult> relatedPlayers,
    IReadOnlyDictionary<long, string> knownNames,
    PlayerPeriodModel period)
{
    private List<PlayerCampaignRowModel>? _campaigns;
    private List<PlayerRelationRowModel>? _relationRows;

    public RankingModel FirstPlayerRanking { get; } = firstPlayerRanking;
    public RankingModel? SecondPlayerRanking { get; } = secondPlayerRanking;
    public IReadOnlyList<RankingPlayer> Players { get; } = players;
    public IReadOnlyList<PlayerConnectionResult> RelatedPlayers { get; } = relatedPlayers;
    public PlayerPeriodModel Period { get; } = period;
    public PlayerSkinsModel? FirstPlayerSkins { get; init; }
    public PlayerSkinsModel? SecondPlayerSkins { get; init; }
    public bool SkinShop { get; init; }
    public bool Administrator { get; init; }
    public TimeZoneInfo TimeZone { get; init; } = TimeZoneInfo.Utc;
    public PlayerCommunityModel? Community { get; set; }
    public PlayerSteamModel FirstPlayerSteam { get; init; } = new(null, TimeZoneInfo.Utc, DateTime.UtcNow);
    public PlayerSteamModel? SecondPlayerSteam { get; init; }

    public IReadOnlyList<MatchCardModel> LastMatches { get; } = matches;

    private Dictionary<long, PlayerRelation> FirstPlayerRelations => FirstPlayerRanking.Player.Relations;
    private Dictionary<long, PlayerRelation> SecondPlayerRelations => SecondPlayerRanking?.Player.Relations ?? [];

    private List<PlayerRelationRowModel> RelationRows => _relationRows ??= BuildRelationRows();

    public List<PlayerCampaignRowModel> Campaigns => _campaigns ??= BuildCampaignRows();

    public PlayerRelationModel? HeadToHead
    {
        get
        {
            if (SecondPlayerRanking == null)
                return null;

            var relation = FirstPlayerRelations.GetValueOrDefault(SecondPlayerRanking.Player.CommunityId);

            return relation == null ? null : new PlayerRelationModel(relation);
        }
    }

    public List<PlayerRelationRowModel> Relations
    {
        get
        {
            var secondPlayerId = SecondPlayerRanking?.Player.CommunityId;

            return
            [
                .. RelationRows
                    .Where(row => row.TogetherGames > 0 || row.AgainstGames > 0)
                    .OrderByDescending(row => row.CommunityId == secondPlayerId)
                    .ThenByDescending(row => row.TogetherGames + row.AgainstGames)
                    .ThenByDescending(row => row.TogetherWins + row.AgainstWins)
            ];
        }
    }

    public DateTime LocalTime(DateTime utc)
    {
        return SiteTime.FromUtc(utc, TimeZone);
    }

    private List<PlayerRelationRowModel> BuildRelationRows()
    {
        var firstPlayerId = FirstPlayerRanking.Player.CommunityId;
        var secondPlayerId = SecondPlayerRanking?.Player.CommunityId;

        var counterpartIds = FirstPlayerRelations.Values
            .Concat(SecondPlayerRelations.Values)
            .Select(relation => relation.CommunityId)
            .Where(communityId => communityId != firstPlayerId)
            .Distinct();

        var rows = new List<PlayerRelationRowModel>();

        foreach (var communityId in counterpartIds)
        {
            var counterpart = Players.FirstOrDefault(player => player.CommunityId == communityId);
            var first = FirstPlayerRelations.GetValueOrDefault(communityId);

            var secondCounterpartId = communityId == secondPlayerId ? firstPlayerId : communityId;
            var second = SecondPlayerRelations.GetValueOrDefault(secondCounterpartId);

            rows.Add(new PlayerRelationRowModel(communityId, counterpart, knownNames.GetValueOrDefault(communityId), first, second));
        }

        return rows;
    }

    private List<PlayerCampaignRowModel> BuildCampaignRows()
    {
        var first = FirstPlayerRanking.Player.Campaigns;
        var second = SecondPlayerRanking?.Player.Campaigns ?? [];

        return
        [
            .. first.Keys
                .Union(second.Keys)
                .Where(name => !string.IsNullOrEmpty(name))
                .Select(name => new PlayerCampaignRowModel(name, first.GetValueOrDefault(name), second.GetValueOrDefault(name)))
                .Where(row => row.Games > 0)
                .OrderByDescending(row => row.Games)
                .ThenByDescending(row => row.First?.Games ?? 0)
                .ThenBy(row => row.Name)
        ];
    }
}
