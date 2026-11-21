using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;
using L4D2PlayStats.Hub.Web.Helpers;

namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PlayerListRow(RankingPlayer player, PlayerResultProperty property)
{
    public RankingPlayer Player => player;
    public long CommunityId => player.CommunityId;
    public string Name => player.MainName;
    public decimal Value { get; } = PlayerProperties.Value(player, property);
    public int Games => player.Games;
    public decimal? PerGame => Games > 0 ? Value / Games : null;
}
