using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Championships;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Models.Championships;
using L4D2PlayStats.Hub.Web.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Services;

public class ChampionshipViewService(IChampionshipQueryService queryService, IChampionshipLoadService loadService, ISiteClock siteClock, IClock clock, ICurrentUser currentUser, IChampionshipJoinService joinService, ISettingsService settingsService) : IChampionshipViewService
{
    public async Task<ChampionshipPageModel> PageAsync(Championship championship, string tab, ChampionshipSeries? series = null, CancellationToken cancellationToken = default)
    {
        var campaigns = await loadService.CampaignsAsync(cancellationToken);
        var me = currentUser.CommunityId;

        return new ChampionshipPageModel
        {
            Championship = championship,
            Profiles = await queryService.ProfilesAsync(championship, cancellationToken),
            Campaigns = campaigns.ToDictionary(campaign => campaign.Id),
            TimeZone = await siteClock.ZoneAsync(cancellationToken),
            Tab = tab,
            CurrentUser = me,
            Administrator = await currentUser.IsAdministratorAsync(),
            Series = series,
            NowUtc = clock.UtcNow,
            JoinOpen = joinService.IsOpen(championship),
            AntiCheatPage = (await settingsService.GetAsync<ModuleSettings>(cancellationToken)).IsEnabled(HubModule.AntiCheat),
            JoinRequest = tab == AppConsts.ChampionshipOverviewTab && me != null && championship.JoinRequests ? await joinService.FindAsync(championship.Id, me.Value, cancellationToken) : null,
            Statistics = tab == AppConsts.ChampionshipStatisticsTab ? await queryService.StatisticsAsync(championship, cancellationToken) : []
        };
    }
}
