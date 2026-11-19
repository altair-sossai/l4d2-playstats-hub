using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.Players;
using L4D2PlayStats.Hub.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers;

[RequireModule(HubModule.Ranking)]
public class PlayersController(IPlayerViewService playerViewService) : Controller
{
    [HttpGet("/players")]
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
    public async Task<IActionResult> Details(long communityId, long? compareWith = null, int? season = null, int? year = null, string? period = null, CancellationToken cancellationToken = default)
    {
        var selected = await playerViewService.PeriodAsync(season, year, period == AppConsts.AllTimeHistoryId, cancellationToken);
        if (selected == null)
            return NotFound();

        var model = communityId <= 0 ? null : await playerViewService.DetailsAsync(selected, communityId, compareWith, cancellationToken);
        if (model == null)
            return View("PlayerNotFound", await playerViewService.NotFoundAsync(selected, communityId, cancellationToken));

        return View(model);
    }
}
