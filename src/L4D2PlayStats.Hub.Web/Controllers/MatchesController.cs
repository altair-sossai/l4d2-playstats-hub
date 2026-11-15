using L4D2PlayStats.Hub.Core.Modules.Matches.Models;
using L4D2PlayStats.Hub.Core.Modules.Matches.Services;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Services;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers;

[RequireModule(HubModule.Ranking)]
public class MatchesController(
    IMatchQueryService matchQueryService,
    ISeasonService seasonService,
    IGameServerService gameServerService,
    IRankingService rankingService,
    ICurrentUser currentUser) : Controller
{
    [HttpGet("/matches")]
    public async Task<IActionResult> Index(int? season, int? server, int page = 1, CancellationToken cancellationToken = default)
    {
        var filter = new MatchFilter { SeasonId = season, GameServerId = server, Page = page, PageSize = AppConsts.MatchesPageSize };

        var model = new MatchListViewModel
        {
            Page = await matchQueryService.ListAsync(filter, cancellationToken),
            Seasons = await seasonService.ListAsync(cancellationToken),
            Servers = await gameServerService.ListAsync(cancellationToken),
            SeasonId = season,
            ServerId = server
        };

        return View(model);
    }

    [HttpGet("/matches/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var match = await matchQueryService.FindAsync(id, cancellationToken);
        if (match == null)
            return NotFound();

        var model = new MatchViewModel
        {
            Match = match,
            Ranking = await rankingService.MatchAsync(id, cancellationToken),
            Administrator = await currentUser.IsAdministratorAsync()
        };

        return View(model);
    }
}
