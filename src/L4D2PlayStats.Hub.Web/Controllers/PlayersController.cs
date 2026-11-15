using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Matches.Models;
using L4D2PlayStats.Hub.Core.Modules.Matches.Services;
using L4D2PlayStats.Hub.Core.Modules.Patents.Models;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services;
using L4D2PlayStats.Hub.Core.Modules.Seasons;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Web.Controllers;

[RequireModule(HubModule.Ranking)]
public class PlayersController(
    HubDbContext dbContext,
    ISeasonService seasonService,
    IRankingService rankingService,
    IMatchQueryService matchQueryService,
    ICurrentUser currentUser) : Controller
{
    private static readonly Dictionary<string, Func<RankingPlayer, decimal>> Orders = new(StringComparer.OrdinalIgnoreCase)
    {
        ["experience"] = player => player.Experience,
        ["games"] = player => player.Games,
        ["winrate"] = player => player.WinRate,
        ["sidamage"] = player => PerGame(player.SiDamage, player),
        ["common"] = player => PerGame(player.Common, player),
        ["sikilled"] = player => PerGame(player.SiKilled, player),
        ["tankdamage"] = player => PerGame(player.TankDamage, player),
        ["skeets"] = player => PerGame(player.Skeets, player),
        ["dmgtotal"] = player => PerGame(player.DmgTotal, player),
        ["ffgiven"] = player => PerGame(player.FfGiven, player),
        ["mvps"] = player => player.Stars
    };

    [HttpGet("/players")]
    public async Task<IActionResult> Index(int? season, string? orderBy, CancellationToken cancellationToken)
    {
        var selected = await SeasonAsync(season, cancellationToken);
        if (selected == null)
            return NotFound();

        var order = orderBy != null && Orders.ContainsKey(orderBy) ? orderBy.ToLowerInvariant() : "experience";
        var players = await rankingService.SeasonAsync(selected.Id, cancellationToken);

        var model = new PlayersViewModel
        {
            Season = selected,
            Seasons = await seasonService.ListAsync(cancellationToken),
            Players = [.. players.OrderByDescending(Orders[order]).ThenBy(player => player.Position)],
            OrderBy = order
        };

        return View(model);
    }

    [HttpGet("/player/{communityId:long}")]
    public async Task<IActionResult> Details(long communityId, int? season, CancellationToken cancellationToken)
    {
        var selected = await SeasonAsync(season, cancellationToken);
        if (selected == null)
            return NotFound();

        var profile = await dbContext.PlayerProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.CommunityId == communityId, cancellationToken);
        if (profile == null)
            return NotFound();

        var ranking = await rankingService.SeasonAsync(selected.Id, cancellationToken);
        var allTime = await rankingService.AllTimeAsync(cancellationToken);
        var stars = await rankingService.StarsAsync(cancellationToken);
        var ranked = ranking.FirstOrDefault(player => player.CommunityId == communityId);
        var matches = await matchQueryService.ListAsync(new MatchFilter { SeasonId = selected.Id, CommunityId = communityId, PageSize = AppConsts.MatchesPageSize }, cancellationToken);

        var model = new PlayerViewModel
        {
            CommunityId = communityId,
            Name = ranked?.MainName ?? profile.Name ?? communityId.ToString(),
            Season = selected,
            Seasons = await seasonService.ListAsync(cancellationToken),
            Ranked = ranked,
            Progress = ranked == null ? null : new PatentProgress(ranked.Experience, ranked.PreviousExperience),
            AllTime = allTime.FirstOrDefault(player => player.CommunityId == communityId),
            Matches = matches.Matches,
            Names = ranking.ToDictionary(player => player.CommunityId, player => player.MainName),
            Stars = stars.GetValueOrDefault(communityId),
            Administrator = await currentUser.IsAdministratorAsync()
        };

        return View(model);
    }

    private async Task<Season?> SeasonAsync(int? season, CancellationToken cancellationToken)
    {
        return season == null
            ? await seasonService.GetCurrentAsync(cancellationToken)
            : await seasonService.FindAsync(season.Value, cancellationToken);
    }

    private static decimal PerGame(int value, RankingPlayer player)
    {
        return player.Games == 0 ? 0 : value / (decimal)player.Games;
    }
}
