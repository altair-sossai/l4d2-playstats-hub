using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Matches;
using L4D2PlayStats.Hub.Core.Modules.Patents;
using L4D2PlayStats.Hub.Core.Modules.Ranking;
using L4D2PlayStats.Hub.Core.Modules.Seasons;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using L4D2PlayStats.Hub.Web.Infrastructure;
using L4D2PlayStats.Hub.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

[RequireModule(HubModule.Ranking)]
public class InGameController(
    ISettingsService settingsService,
    ISeasonService seasonService,
    IRankingService rankingService,
    IMatchQueryService matchQueryService) : Controller
{
    [HttpGet("/ingame")]
    public async Task<IActionResult> Index(long? steamid, CancellationToken cancellationToken)
    {
        return View(await ModelAsync(steamid, false, cancellationToken));
    }

    [HttpGet("/ingame/player")]
    public async Task<IActionResult> Player(long? steamid, CancellationToken cancellationToken)
    {
        return View(await ModelAsync(steamid, false, cancellationToken));
    }

    [HttpGet("/ingame/matches")]
    public async Task<IActionResult> Matches(long? steamid, CancellationToken cancellationToken)
    {
        return View(await ModelAsync(steamid, true, cancellationToken));
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
            Matches = page?.Matches ?? [],
            SteamId = steamId
        };
    }
}
