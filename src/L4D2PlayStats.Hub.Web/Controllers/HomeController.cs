using L4D2PlayStats.Hub.Core.Modules.Ranking.Services;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Web.Infrastructure.Localization;
using L4D2PlayStats.Hub.Web.Models.Ranking;
using L4D2PlayStats.Hub.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace L4D2PlayStats.Hub.Web.Controllers;

public class HomeController(
    ISettingsService settingsService,
    ISeasonService seasonService,
    IRankingService rankingService,
    IRankingHistoryService rankingHistoryService,
    IStringLocalizer<SharedResource> localizer) : Controller
{
    [HttpGet("/")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        if (!modules.IsEnabled(HubModule.Ranking))
            return View("Welcome");

        ViewBag.Home = "active";

        var season = await seasonService.GetCurrentAsync(cancellationToken);
        var now = await seasonService.SiteNowAsync(cancellationToken);
        var players = await rankingService.SeasonAsync(season.Id, cancellationToken);
        var nextResetIn = (int)Math.Ceiling((season.EndsAt - now).TotalDays);

        if (players.Count > 0)
            return View(new HomeModel { Ranking = [.. players.Select(player => new RankingModel(localizer, player))], NextResetIn = nextResetIn });

        return View(new HomeModel { NextResetIn = nextResetIn, LastHistory = await rankingHistoryService.LastAsync(cancellationToken) });
    }
}
