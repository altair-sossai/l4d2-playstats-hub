using L4D2PlayStats.Hub.Core.Modules.Seasons.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Localization;
using L4D2PlayStats.Hub.Web.Models.Ranking;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Localization;

namespace L4D2PlayStats.Hub.Web.Controllers;

public class HomeController(
    ISettingsService settingsService,
    ISeasonService seasonService,
    IPlayerViewService playerViewService,
    IRankingHistoryService rankingHistoryService,
    IStringLocalizer<SharedResource> localizer) : Controller
{
    [HttpGet("/")]
    [OutputCache(PolicyName = AppConsts.PageCachePolicy)]
    public async Task<IActionResult> Index(int? season = null, int? year = null, string? period = null, CancellationToken cancellationToken = default)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        if (!modules.IsEnabled(HubModule.Ranking))
            return View("Welcome");

        ViewBag.Home = "active";

        var selected = await playerViewService.PeriodAsync(season, year, period == AppConsts.AllTimeHistoryId, cancellationToken);
        if (selected == null)
            return NotFound();

        var now = await seasonService.SiteNowAsync(cancellationToken);
        var players = await playerViewService.RankingAsync(selected, cancellationToken);
        var nextResetIn = (int)Math.Ceiling((selected.CurrentSeason.EndsAt - now).TotalDays);

        return View(new HomeModel
        {
            Ranking = [.. players.Select(player => new RankingModel(localizer, player))],
            NextResetIn = nextResetIn,
            Period = selected,
            LastHistory = players.Count == 0 && selected.IsCurrent ? await rankingHistoryService.LastAsync(cancellationToken) : null
        });
    }
}
