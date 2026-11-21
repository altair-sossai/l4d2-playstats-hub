using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers;

[RequireModule(HubModule.Ranking)]
public class HistoryController(IRankingHistoryService rankingHistoryService) : Controller
{
    [HttpGet("/history")]
    public IActionResult Index()
    {
        return RedirectPermanent(PlayerUrls.Summary(null, false, null));
    }

    [HttpGet("/history/{historyId}")]
    public async Task<IActionResult> Period(string historyId, CancellationToken cancellationToken)
    {
        return ToPlayers(await rankingHistoryService.PlayersPeriodAsync(historyId, cancellationToken));
    }

    [HttpGet("/ranking/season/{id:int}")]
    public async Task<IActionResult> Season(int id, CancellationToken cancellationToken)
    {
        return ToPlayers(await rankingHistoryService.SeasonPeriodAsync(id, cancellationToken));
    }

    [HttpGet("/ranking/year/{year:int}")]
    public async Task<IActionResult> Year(int year, CancellationToken cancellationToken)
    {
        return ToPlayers(await rankingHistoryService.YearPeriodAsync(year, cancellationToken));
    }

    [HttpGet("/ranking/all-time")]
    public IActionResult AllTime()
    {
        return ToPlayers(PlayerUrls.AllTimeQuery());
    }

    private IActionResult ToPlayers(string? period)
    {
        return period == null ? NotFound() : RedirectPermanent(PlayerUrls.Summary(null, false, period));
    }
}
