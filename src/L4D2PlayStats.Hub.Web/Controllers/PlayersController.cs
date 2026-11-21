using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.Players;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace L4D2PlayStats.Hub.Web.Controllers;

public class PlayersController(IPlayerViewService playerViewService, IPlayerCommunityService playerCommunityService, ISettingsService settingsService) : Controller
{
    [RequireModule(HubModule.Ranking)]
    [HttpGet("/players")]
    [OutputCache(PolicyName = AppConsts.PageCachePolicy)]
    public async Task<IActionResult> Index(PlayerResultProperty? orderBy = null, PlayerResultProperty? sort = null, bool asc = false, int? season = null, int? year = null, string? period = null, int page = 1, CancellationToken cancellationToken = default)
    {
        ViewBag.Players = "active";

        var selected = await playerViewService.PeriodAsync(season, year, period == AppConsts.AllTimeHistoryId, cancellationToken, true);
        if (selected == null)
            return NotFound();

        if (orderBy != null)
            return View(await playerViewService.ListAsync(selected, Enum.IsDefined(orderBy.Value) ? orderBy.Value : PlayerResultProperty.Experience, asc, page, cancellationToken));

        if (sort != null && !PlayerProperties.SummaryColumns.Contains(sort.Value))
            sort = null;

        return View(await playerViewService.SummaryAsync(selected, sort, asc, page, cancellationToken));
    }

    [HttpGet("/player/{communityId:long}/{compareWith:long?}")]
    [OutputCache(PolicyName = AppConsts.PageCachePolicy, Tags = [AppConsts.CommunityCacheTag])]
    public async Task<IActionResult> Details(long communityId, long? compareWith = null, int? season = null, int? year = null, string? period = null, CancellationToken cancellationToken = default)
    {
        if (!SteamIds.IsValid(communityId) || (compareWith != null && !SteamIds.IsValid(compareWith.Value)))
            return NotFound();

        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        if (!modules.IsEnabled(HubModule.Ranking))
            return await PlayerNotFoundAsync(null, communityId, cancellationToken);

        var selected = await playerViewService.PeriodAsync(season, year, period == AppConsts.AllTimeHistoryId, cancellationToken);
        if (selected == null)
            return NotFound();

        var model = await playerViewService.DetailsAsync(selected, communityId, compareWith, cancellationToken);
        if (model == null)
            return await PlayerNotFoundAsync(selected, communityId, cancellationToken);

        model.Community = await playerCommunityService.BuildAsync(communityId, compareWith, cancellationToken);

        return View(model);
    }

    private async Task<IActionResult> PlayerNotFoundAsync(PlayerPeriodModel? period, long communityId, CancellationToken cancellationToken)
    {
        var model = await playerViewService.NotFoundAsync(period, communityId, cancellationToken);

        model.Community = await playerCommunityService.BuildAsync(communityId, null, cancellationToken);

        return View(AppConsts.PlayerNotFoundView, model);
    }
}
