using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Matches.Models;
using L4D2PlayStats.Hub.Core.Modules.Matches.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Commands.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace L4D2PlayStats.Hub.Web.Controllers;

[RequireModule(HubModule.Ranking)]
public class InGameController(
    ISettingsService settingsService,
    ISeasonService seasonService,
    IRankingService rankingService,
    IMatchQueryService matchQueryService,
    ICommandCatalog commandCatalog) : Controller
{
    [HttpGet(AppConsts.InGamePath)]
    [OutputCache(PolicyName = AppConsts.PageCachePolicy)]
    public async Task<IActionResult> Index(long? steamid, CancellationToken cancellationToken)
    {
        return View(await ModelAsync(steamid, false, cancellationToken));
    }

    [HttpGet(AppConsts.InGamePlayerPath)]
    [OutputCache(PolicyName = AppConsts.PageCachePolicy)]
    public async Task<IActionResult> Player(long? steamid, CancellationToken cancellationToken)
    {
        return View(await ModelAsync(steamid, false, cancellationToken));
    }

    [HttpGet(AppConsts.InGameMatchesPath)]
    [OutputCache(PolicyName = AppConsts.PageCachePolicy)]
    public async Task<IActionResult> Matches(long? steamid, CancellationToken cancellationToken)
    {
        return View(await ModelAsync(steamid, true, cancellationToken));
    }

    [HttpGet(AppConsts.InGameCommandsPath)]
    [OutputCache(PolicyName = AppConsts.PageCachePolicy)]
    public async Task<IActionResult> Commands(long? steamid, CancellationToken cancellationToken)
    {
        var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);
        var season = await seasonService.GetCurrentAsync(cancellationToken);
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        var sections = commandCatalog.Sections(CultureInfo.CurrentUICulture, modules)
            .Where(section => !section.Admin)
            .Select(section => section with { Commands = [.. section.Commands.Where(command => !command.Admin)] })
            .Where(section => section.Commands.Count > 0)
            .ToList();

        return View(new InGameCommandsViewModel { SiteName = site.Name, Season = season, SteamId = steamid, Sections = sections });
    }

    private async Task<InGameViewModel> ModelAsync(long? steamId, bool matches, CancellationToken cancellationToken)
    {
        var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);
        var season = await seasonService.GetCurrentAsync(cancellationToken);
        var ranking = await rankingService.SeasonAsync(season.Id, cancellationToken);
        var me = steamId == null ? null : ranking.FirstOrDefault(player => player.CommunityId == steamId);

        var page = matches
            ? await matchQueryService.ListAsync(new MatchFilter { SeasonId = season.Id, RankingOnly = true, PageSize = AppConsts.InGameMatches }, cancellationToken)
            : null;

        return new InGameViewModel
        {
            SiteName = site.Name,
            Season = season,
            Players = [.. ranking.Take(AppConsts.InGameRankingPlayers).Select(player => new RankingRow(player))],
            Me = me == null ? null : new RankingRow(me),
            Matches = page?.Items ?? [],
            SteamId = steamId
        };
    }
}
