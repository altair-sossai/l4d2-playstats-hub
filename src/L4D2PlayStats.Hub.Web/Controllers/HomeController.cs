using L4D2PlayStats.Hub.Core.Modules.Patents.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services;
using L4D2PlayStats.Hub.Core.Modules.Seasons;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Enums;
using L4D2PlayStats.Hub.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers;

public class HomeController(
    ISettingsService settingsService,
    ISeasonService seasonService,
    IRankingService rankingService,
    ICurrentUser currentUser) : Controller
{
    [HttpGet("/")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        if (!modules.IsEnabled(HubModule.Ranking))
            return View("Welcome");

        var season = await seasonService.GetCurrentAsync(cancellationToken);

        return await RankingAsync(RankingScope.Season, season, null, true, cancellationToken);
    }

    [HttpGet("/ranking/season/{id:int}")]
    [RequireModule(HubModule.Ranking)]
    public async Task<IActionResult> Season(int id, CancellationToken cancellationToken)
    {
        var season = await seasonService.FindAsync(id, cancellationToken);
        if (season == null)
            return NotFound();

        var current = await seasonService.GetCurrentAsync(cancellationToken);

        return await RankingAsync(RankingScope.Season, season, null, current.Id == season.Id, cancellationToken);
    }

    [HttpGet("/ranking/year/{year:int}")]
    [RequireModule(HubModule.Ranking)]
    public Task<IActionResult> Year(int year, CancellationToken cancellationToken)
    {
        return RankingAsync(RankingScope.Year, null, year, false, cancellationToken);
    }

    [HttpGet("/ranking/all-time")]
    [RequireModule(HubModule.Ranking)]
    public Task<IActionResult> AllTime(CancellationToken cancellationToken)
    {
        return RankingAsync(RankingScope.AllTime, null, null, false, cancellationToken);
    }

    [HttpGet("/patents")]
    [RequireModule(HubModule.Ranking)]
    public async Task<IActionResult> Patents(CancellationToken cancellationToken)
    {
        ViewBag.Settings = await settingsService.GetAsync<RankingSettings>(cancellationToken);

        return View(PatentCatalog.All);
    }

    private async Task<IActionResult> RankingAsync(RankingScope scope, Season? season, int? year, bool current, CancellationToken cancellationToken)
    {
        var seasons = await seasonService.ListAsync(cancellationToken);

        var players = scope switch
        {
            RankingScope.Season => await rankingService.SeasonAsync(season!.Id, cancellationToken),
            RankingScope.Year => await rankingService.YearAsync(year!.Value, cancellationToken),
            _ => await rankingService.AllTimeAsync(cancellationToken)
        };

        var model = new RankingViewModel
        {
            Scope = scope,
            Season = season,
            Year = year,
            Current = current,
            Seasons = seasons,
            Years = [.. seasons.Select(s => s.StartsAt.Year).Distinct().OrderByDescending(y => y)],
            Players = [.. players.Select(player => new RankingRow(player))],
            Highlight = currentUser.CommunityId
        };

        return View("Ranking", model);
    }
}
