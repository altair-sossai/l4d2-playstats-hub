using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Administrators;
using L4D2PlayStats.Hub.Core.Modules.Campaigns;
using L4D2PlayStats.Hub.Core.Modules.GameModes;
using L4D2PlayStats.Hub.Core.Modules.Matches;
using L4D2PlayStats.Hub.Core.Modules.Players;
using L4D2PlayStats.Hub.Core.Modules.Punishments;
using L4D2PlayStats.Hub.Core.Modules.Seasons;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles;
using L4D2PlayStats.Hub.Web.Infrastructure;
using L4D2PlayStats.Hub.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

public interface IAdminViewService
{
    Task<ServerStatusViewModel> StatusAsync(GameServer server, CancellationToken cancellationToken = default);
    Task<SettingsViewModel> SettingsAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(SettingsViewModel model, CancellationToken cancellationToken = default);
    string SiteUrl(HttpRequest request, SiteSettings site);
}

public class AdminViewService(HubDbContext dbContext, IGameServerPresence presence, ISettingsService settingsService) : IAdminViewService
{
    public async Task<ServerStatusViewModel> StatusAsync(GameServer server, CancellationToken cancellationToken = default)
    {
        return new ServerStatusViewModel
        {
            Server = server,
            Status = presence.GetStatus(server),
            Modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken),
            Files = await dbContext.StatisticsFiles.CountAsync(f => f.GameServerId == server.Id, cancellationToken)
        };
    }

    public async Task<SettingsViewModel> SettingsAsync(CancellationToken cancellationToken = default)
    {
        return new SettingsViewModel
        {
            Site = await settingsService.GetAsync<SiteSettings>(cancellationToken),
            Season = await settingsService.GetAsync<SeasonSettings>(cancellationToken),
            Ranking = await settingsService.GetAsync<RankingSettings>(cancellationToken),
            TimeZones = [.. TimeZoneInfo.GetSystemTimeZones().Select(zone => zone.Id).OrderBy(id => id)]
        };
    }

    public async Task SaveAsync(SettingsViewModel model, CancellationToken cancellationToken = default)
    {
        var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);

        site.Name = string.IsNullOrWhiteSpace(model.Site.Name) ? site.Name : model.Site.Name.Trim();
        site.PublicUrl = string.IsNullOrWhiteSpace(model.Site.PublicUrl) ? null : model.Site.PublicUrl.Trim().TrimEnd('/');
        site.SteamApiKey = string.IsNullOrWhiteSpace(model.Site.SteamApiKey) ? null : model.Site.SteamApiKey.Trim();

        if (!string.IsNullOrWhiteSpace(model.Site.TimeZone) && TimeZoneInfo.TryFindSystemTimeZoneById(model.Site.TimeZone.Trim(), out _))
            site.TimeZone = model.Site.TimeZone.Trim();

        if (AppConsts.SupportedCultures.Contains(model.Site.DefaultCulture))
            site.DefaultCulture = model.Site.DefaultCulture;

        var season = new SeasonSettings { LengthInMonths = SeasonCalendar.NormalizeLength(model.Season.LengthInMonths) };

        var ranking = model.Ranking;
        ranking.MinimumMatches = Math.Max(0, ranking.MinimumMatches);

        await settingsService.SaveAsync(site, cancellationToken);
        await settingsService.SaveAsync(season, cancellationToken);
        await settingsService.SaveAsync(ranking, cancellationToken);
    }

    public string SiteUrl(HttpRequest request, SiteSettings site)
    {
        return string.IsNullOrEmpty(site.PublicUrl) ? $"{request.Scheme}://{request.Host}" : site.PublicUrl;
    }
}

[Authorize(Policy = AppConsts.AdministratorPolicy)]
[AutoValidateAntiforgeryToken]
[Route("admin")]
public class AdminController(
    HubDbContext dbContext,
    IAdminViewService adminViewService,
    IGameServerService gameServerService,
    IGameModeService gameModeService,
    ICampaignService campaignService,
    IAdministratorService administratorService,
    ISettingsService settingsService,
    IMatchBuildService matchBuildService,
    IPunishmentService punishmentService,
    IStatisticsService statisticsService,
    ICurrentUser currentUser) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var servers = new List<ServerStatusViewModel>();

        foreach (var server in await gameServerService.ListAsync(cancellationToken))
            servers.Add(await adminViewService.StatusAsync(server, cancellationToken));

        var ruleSet = await gameModeService.GetRuleSetAsync(cancellationToken);
        var configurations = await gameModeService.ListConfigurationsAsync(cancellationToken);

        var model = new DashboardViewModel
        {
            Servers = servers,
            Files = await dbContext.StatisticsFiles.CountAsync(cancellationToken),
            Matches = await dbContext.Matches.CountAsync(cancellationToken),
            RankedMatches = await dbContext.Matches.CountAsync(m => m.CountsForRanking, cancellationToken),
            Players = await dbContext.PlayerProfiles.CountAsync(cancellationToken),
            Seasons = await dbContext.Seasons.CountAsync(cancellationToken),
            UnmatchedConfigurations = configurations.Count(c => ruleSet.Find(c.Name, c.TeamSize) == null)
        };

        return View(model);
    }

    [HttpGet("servers")]
    public async Task<IActionResult> Servers(CancellationToken cancellationToken)
    {
        var servers = new List<ServerStatusViewModel>();

        foreach (var server in await gameServerService.ListAsync(cancellationToken))
            servers.Add(await adminViewService.StatusAsync(server, cancellationToken));

        return View(servers);
    }

    [HttpPost("servers")]
    public async Task<IActionResult> CreateServer(ServerForm form, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(form.Name))
        {
            TempData["Error"] = "A name is required.";
            return RedirectToAction(nameof(Servers));
        }

        var credentials = await gameServerService.CreateAsync(form.Name, form.Address, cancellationToken);
        TempData["Token"] = credentials.Token;

        return RedirectToAction(nameof(Server), new { id = credentials.Server.Id });
    }

    [HttpGet("servers/{id:int}")]
    public async Task<IActionResult> Server(int id, CancellationToken cancellationToken)
    {
        var server = await gameServerService.FindAsync(id, cancellationToken);
        if (server == null)
            return NotFound();

        var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);

        var model = new ServerViewModel
        {
            Status = await adminViewService.StatusAsync(server, cancellationToken),
            Token = TempData["Token"] as string,
            SiteUrl = adminViewService.SiteUrl(Request, site)
        };

        return View(model);
    }

    [HttpGet("servers/{id:int}/status")]
    public async Task<IActionResult> ServerStatus(int id, CancellationToken cancellationToken)
    {
        var server = await gameServerService.FindAsync(id, cancellationToken);
        if (server == null)
            return NotFound();

        return PartialView("_ServerStatus", await adminViewService.StatusAsync(server, cancellationToken));
    }

    [HttpPost("servers/{id:int}")]
    public async Task<IActionResult> UpdateServer(int id, ServerForm form, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(form.Name))
            await gameServerService.UpdateAsync(id, form.Name, form.Address, form.Enabled, cancellationToken);

        return RedirectToAction(nameof(Server), new { id });
    }

    [HttpPost("servers/{id:int}/key")]
    public async Task<IActionResult> RegenerateKey(int id, CancellationToken cancellationToken)
    {
        var credentials = await gameServerService.RegenerateKeyAsync(id, cancellationToken);
        if (credentials == null)
            return NotFound();

        TempData["Token"] = credentials.Token;

        return RedirectToAction(nameof(Server), new { id });
    }

    [HttpPost("servers/{id:int}/delete")]
    public async Task<IActionResult> DeleteServer(int id, CancellationToken cancellationToken)
    {
        if (!await gameServerService.DeleteAsync(id, cancellationToken))
            TempData["Error"] = "A server that already sent statistics cannot be deleted. Disable it instead.";

        return RedirectToAction(nameof(Servers));
    }

    [HttpGet("settings")]
    public async Task<IActionResult> Settings(CancellationToken cancellationToken)
    {
        return View(await adminViewService.SettingsAsync(cancellationToken));
    }

    [HttpPost("settings")]
    public async Task<IActionResult> SaveSettings(SettingsViewModel model, CancellationToken cancellationToken)
    {
        await adminViewService.SaveAsync(model, cancellationToken);
        TempData["Saved"] = true;

        return RedirectToAction(nameof(Settings));
    }

    [HttpGet("modules")]
    public async Task<IActionResult> Modules(CancellationToken cancellationToken)
    {
        return View(await settingsService.GetAsync<ModuleSettings>(cancellationToken));
    }

    [HttpPost("modules")]
    public async Task<IActionResult> SaveModules(ModuleSettings modules, CancellationToken cancellationToken)
    {
        await settingsService.SaveAsync(modules, cancellationToken);
        TempData["Saved"] = true;

        return RedirectToAction(nameof(Modules));
    }

    [HttpGet("game-modes")]
    public async Task<IActionResult> GameModes(CancellationToken cancellationToken)
    {
        var rules = await gameModeService.ListRulesAsync(cancellationToken);
        var ruleSet = new GameModeRuleSet(rules);
        var configurations = await gameModeService.ListConfigurationsAsync(cancellationToken);

        var model = new GameModesViewModel
        {
            Rules = rules,
            Configurations = [.. configurations.Select(c => new SeenConfiguration(c, ruleSet.Find(c.Name, c.TeamSize)))]
        };

        return View(model);
    }

    [HttpPost("game-modes")]
    public async Task<IActionResult> SaveGameMode(GameModeRule rule, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(rule.Name))
            await gameModeService.SaveRuleAsync(rule, cancellationToken);

        return RedirectToAction(nameof(GameModes));
    }

    [HttpPost("game-modes/{id:int}/delete")]
    public async Task<IActionResult> DeleteGameMode(int id, CancellationToken cancellationToken)
    {
        await gameModeService.DeleteRuleAsync(id, cancellationToken);

        return RedirectToAction(nameof(GameModes));
    }

    [HttpGet("campaigns")]
    public async Task<IActionResult> Campaigns(CancellationToken cancellationToken)
    {
        return View(new CampaignsViewModel { Campaigns = await campaignService.ListAsync(cancellationToken) });
    }

    [HttpPost("campaigns")]
    public async Task<IActionResult> SaveCampaign(CampaignForm form, CancellationToken cancellationToken)
    {
        var maps = (form.Maps ?? string.Empty).Split(['\n', '\r', ',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (!string.IsNullOrWhiteSpace(form.Name) && maps.Length > 0)
            await campaignService.SaveAsync(form.Id, form.Name, maps, cancellationToken);

        return RedirectToAction(nameof(Campaigns));
    }

    [HttpPost("campaigns/{id:int}/delete")]
    public async Task<IActionResult> DeleteCampaign(int id, CancellationToken cancellationToken)
    {
        await campaignService.DeleteAsync(id, cancellationToken);

        return RedirectToAction(nameof(Campaigns));
    }

    [HttpGet("administrators")]
    public async Task<IActionResult> Administrators(CancellationToken cancellationToken)
    {
        return View(new AdministratorsViewModel { Administrators = await administratorService.ListAsync(cancellationToken), Current = currentUser.CommunityId });
    }

    [HttpPost("administrators")]
    public async Task<IActionResult> AddAdministrator(long communityId, string? name, CancellationToken cancellationToken)
    {
        if (SteamIds.IsValid(communityId))
            await administratorService.AddAsync(communityId, name, cancellationToken);
        else
            TempData["Error"] = "That is not a valid SteamID64.";

        return RedirectToAction(nameof(Administrators));
    }

    [HttpPost("administrators/{communityId:long}/delete")]
    public async Task<IActionResult> RemoveAdministrator(long communityId, CancellationToken cancellationToken)
    {
        if (communityId == currentUser.CommunityId || !await administratorService.RemoveAsync(communityId, cancellationToken))
            TempData["Error"] = "You cannot remove yourself or the last administrator.";

        return RedirectToAction(nameof(Administrators));
    }

    [HttpPost("reprocess")]
    public async Task<IActionResult> Reprocess(CancellationToken cancellationToken)
    {
        TempData["Reprocessed"] = await matchBuildService.RebuildAllAsync(cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("punishments")]
    public async Task<IActionResult> AddPunishment(int seasonId, long communityId, int points, string? reason, CancellationToken cancellationToken)
    {
        await punishmentService.AddAsync(seasonId, communityId, points, reason, currentUser.CommunityId, cancellationToken);

        return Redirect($"/player/{communityId}?season={seasonId}");
    }

    [HttpPost("punishments/delete")]
    public async Task<IActionResult> RemovePunishment(int seasonId, long communityId, CancellationToken cancellationToken)
    {
        await punishmentService.RemoveAsync(seasonId, communityId, cancellationToken);

        return Redirect($"/player/{communityId}?season={seasonId}");
    }

    [HttpPost("statistics/{fileId:int}/score")]
    public async Task<IActionResult> UpdateScore(int fileId, int teamAScore, int teamBScore, CancellationToken cancellationToken)
    {
        var file = await statisticsService.UpdateScoreAsync(fileId, teamAScore, teamBScore, cancellationToken);
        if (file == null)
            return BadRequest();

        var updated = await statisticsService.FindAsync(fileId, cancellationToken);

        return updated?.MatchId == null ? Redirect("/matches") : Redirect($"/matches/{updated.MatchId}");
    }
}
