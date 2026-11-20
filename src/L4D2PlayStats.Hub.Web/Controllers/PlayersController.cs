using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.Players;
using L4D2PlayStats.Hub.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace L4D2PlayStats.Hub.Web.Controllers;

public class PlayersController(IPlayerViewService playerViewService, ISettingsService settingsService) : Controller
{
    [RequireModule(HubModule.Ranking)]
    [HttpGet("/players")]
    [OutputCache(PolicyName = AppConsts.PageCachePolicy)]
    public async Task<IActionResult> Index(PlayerResultProperty orderBy = PlayerResultProperty.Experience, bool asc = false, int? season = null, int? year = null, string? period = null, int page = 1, CancellationToken cancellationToken = default)
    {
        ViewBag.Players = "active";

        var selected = await playerViewService.PeriodAsync(season, year, period == AppConsts.AllTimeHistoryId, cancellationToken);
        if (selected == null)
            return NotFound();

        if (!Enum.IsDefined(orderBy))
            orderBy = PlayerResultProperty.Experience;

        return View(await playerViewService.ListAsync(selected, orderBy, asc, page, cancellationToken));
    }

    [HttpGet("/player/{communityId:long}/{compareWith:long?}")]
    [OutputCache(PolicyName = AppConsts.PageCachePolicy)]
    public async Task<IActionResult> Details(long communityId, long? compareWith = null, int? season = null, int? year = null, string? period = null, CancellationToken cancellationToken = default)
    {
        if (!SteamIds.IsValid(communityId) || (compareWith != null && !SteamIds.IsValid(compareWith.Value)))
            return NotFound();

        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        if (!modules.IsEnabled(HubModule.Ranking))
            return View(AppConsts.PlayerNotFoundView, await playerViewService.NotFoundAsync(null, communityId, cancellationToken));

        var selected = await playerViewService.PeriodAsync(season, year, period == AppConsts.AllTimeHistoryId, cancellationToken);
        if (selected == null)
            return NotFound();

        var model = await playerViewService.DetailsAsync(selected, communityId, compareWith, cancellationToken);
        if (model == null)
            return View(AppConsts.PlayerNotFoundView, await playerViewService.NotFoundAsync(selected, communityId, cancellationToken));

        return View(model);
    }
}
