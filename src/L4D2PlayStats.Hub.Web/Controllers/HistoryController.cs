using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers;

[RequireModule(HubModule.Ranking)]
public class HistoryController(IRankingHistoryService rankingHistoryService) : Controller
{
    [HttpGet("/history/{historyId?}")]
    public async Task<IActionResult> Index(string? historyId = null, CancellationToken cancellationToken = default)
    {
        var period = await rankingHistoryService.PlayersPeriodAsync(historyId, cancellationToken);

        return RedirectPermanent(PlayerUrls.Summary(null, false, period));
    }

    [HttpGet("/ranking/season/{id:int}")]
    public IActionResult Season(int id)
    {
        return RedirectPermanent(PlayerUrls.Summary(null, false, PlayerUrls.SeasonQuery(id)));
    }

    [HttpGet("/ranking/year/{year:int}")]
    public IActionResult Year(int year)
    {
        return RedirectPermanent(PlayerUrls.Summary(null, false, PlayerUrls.YearQuery(year)));
    }

    [HttpGet("/ranking/all-time")]
    public IActionResult AllTime()
    {
        return RedirectPermanent(PlayerUrls.Summary(null, false, PlayerUrls.AllTimeQuery()));
    }
}
