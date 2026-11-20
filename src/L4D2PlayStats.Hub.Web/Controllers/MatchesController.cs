using L4D2PlayStats.Hub.Core.Modules.Matches.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace L4D2PlayStats.Hub.Web.Controllers;

[RequireModule(HubModule.Ranking)]
public class MatchesController(IMatchViewService matchViewService, IMatchResultService matchResultService) : Controller
{
    [HttpGet("/matches")]
    [OutputCache(PolicyName = AppConsts.PageCachePolicy)]
    public async Task<IActionResult> Index(int? season, int? year, int? server, int page = 1, CancellationToken cancellationToken = default)
    {
        ViewBag.LastMatches = "active";

        var model = await matchViewService.ListAsync(season, year, server, page, cancellationToken);

        return View(model);
    }

    [HttpGet("/matches/{id:int}")]
    [OutputCache(PolicyName = AppConsts.PageCachePolicy)]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var model = await matchViewService.DetailsAsync(id, cancellationToken);

        return model == null ? NotFound() : View(model);
    }

    [HttpGet("/matches/{id:int}/statistics/{fileId:int}")]
    [OutputCache(PolicyName = AppConsts.PageCachePolicy)]
    public async Task<IActionResult> Statistics(int id, int fileId, CancellationToken cancellationToken)
    {
        var model = await matchViewService.StatisticsAsync(id, fileId, cancellationToken);

        return model == null ? NotFound() : View(model);
    }

    [HttpGet("/last-matches")]
    public IActionResult LastMatches()
    {
        return RedirectPermanent(AppConsts.MatchesUrl);
    }

    [HttpGet("/last-matches/details/{start}/{end}")]
    public async Task<IActionResult> LastMatchDetails(string start, string end, CancellationToken cancellationToken)
    {
        var id = await FindIdAsync(start, end, cancellationToken);

        return id == null ? NotFound() : RedirectPermanent($"{AppConsts.MatchesUrl}/{id}");
    }

    [HttpGet("/last-matches/details/{start}/{end}/statistics/{statisticId}")]
    public async Task<IActionResult> LastMatchStatistics(string start, string end, string statisticId, CancellationToken cancellationToken)
    {
        var id = await FindIdAsync(start, end, cancellationToken);
        var fileDate = StatisticsRowKey.ToDate(statisticId);

        if (id == null || fileDate == null)
            return NotFound();

        var fileId = await matchResultService.FindFileIdAsync(id.Value, fileDate.Value, cancellationToken);

        return fileId == null ? NotFound() : RedirectPermanent($"{AppConsts.MatchesUrl}/{id}/statistics/{fileId}");
    }

    private async Task<int?> FindIdAsync(string start, string end, CancellationToken cancellationToken)
    {
        var lastFileDate = StatisticsRowKey.ToDate(start);
        var firstFileDate = StatisticsRowKey.ToDate(end);

        if (lastFileDate == null || firstFileDate == null)
            return null;

        return await matchResultService.FindIdAsync(firstFileDate.Value, lastFileDate.Value, cancellationToken);
    }
}