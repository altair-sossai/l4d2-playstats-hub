using L4D2PlayStats.Hub.Core.Modules.Championships;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Models.Championships;
using L4D2PlayStats.Hub.Web.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Services;

public class ChampionshipViewService(IChampionshipQueryService queryService, IChampionshipLoadService loadService, ISiteClock siteClock, ICurrentUser currentUser) : IChampionshipViewService
{
    public async Task<ChampionshipPageModel> PageAsync(Championship championship, string tab, ChampionshipSeries? series = null, CancellationToken cancellationToken = default)
    {
        var campaigns = await loadService.CampaignsAsync(cancellationToken);

        return new ChampionshipPageModel
        {
            Championship = championship,
            Profiles = await queryService.ProfilesAsync(championship, cancellationToken),
            Campaigns = campaigns.ToDictionary(campaign => campaign.Id),
            TimeZone = await siteClock.ZoneAsync(cancellationToken),
            Tab = tab,
            CurrentUser = currentUser.CommunityId,
            Administrator = await currentUser.IsAdministratorAsync(),
            Series = series,
            Statistics = tab == AppConsts.ChampionshipStatisticsTab ? await queryService.StatisticsAsync(championship, cancellationToken) : []
        };
    }
}
