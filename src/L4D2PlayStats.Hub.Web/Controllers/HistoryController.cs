using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers;

[RequireModule(HubModule.Ranking)]
public class HistoryController(IRankingHistoryService rankingHistoryService) : Controller
{
    [HttpGet("/history/{historyId?}")]
    public async Task<IActionResult> Index(string? historyId = null, CancellationToken cancellationToken = default)
    {
        ViewBag.History = "active";

        var model = await rankingHistoryService.FindAsync(historyId, cancellationToken);

        return model == null ? View("NoHistory") : View(model);
    }

    [HttpGet("/ranking/season/{id:int}")]
    public async Task<IActionResult> Season(int id, CancellationToken cancellationToken)
    {
        var historyId = await rankingHistoryService.SeasonHistoryIdAsync(id, cancellationToken);

        return historyId == null ? NotFound() : RedirectToAction(nameof(Index), new { historyId });
    }

    [HttpGet("/ranking/year/{year:int}")]
    public IActionResult Year(int year)
    {
        return RedirectToAction(nameof(Index), new { historyId = year.ToString() });
    }

    [HttpGet("/ranking/all-time")]
    public IActionResult AllTime()
    {
        return RedirectToAction(nameof(Index), new { historyId = AppConsts.AllTimeHistoryId });
    }
}
