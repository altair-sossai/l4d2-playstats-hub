using L4D2PlayStats.Hub.Core.Modules.Patents.Models;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;

namespace L4D2PlayStats.Hub.Web.Models;

public class RankingRow(RankingPlayer player)
{
    public RankingPlayer Player { get; } = player;
    public PatentProgress Progress { get; } = new(player.Experience, player.PreviousExperience);
}