using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;
using L4D2PlayStats.Hub.Web.Infrastructure.Localization;
using Microsoft.Extensions.Localization;

namespace L4D2PlayStats.Hub.Web.Models.Ranking;

public class RankingModel(IStringLocalizer<SharedResource> localizer, RankingPlayer player)
{
    public RankingPlayer Player { get; } = player;
    public PatentProgressModel PatentProgress { get; } = new(localizer, player);
}
