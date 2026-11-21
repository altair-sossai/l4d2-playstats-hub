using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Administrators;
using L4D2PlayStats.Hub.Core.Modules.Campaigns;
using L4D2PlayStats.Hub.Core.Modules.Administrators.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Campaigns.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.GameModes;
using L4D2PlayStats.Hub.Core.Modules.GameModes.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live.Settings;
using L4D2PlayStats.Hub.Core.Modules.Matches.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

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
    ISkinCatalogService skinCatalogService,
    ILiveSettings liveSettings,
    ICurrentUser currentUser) : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewBag.Admin = "active";
    }

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
    public async Task<IActionResult> Servers(int page = 1, CancellationToken cancellationToken = default)
    {
        var size = AppConsts.GameServersPageSize;
        var total = await gameServerService.CountAsync(cancellationToken);
        var current = Paging.Page(page, total, size);
        var servers = new List<ServerStatusViewModel>();

        foreach (var server in await gameServerService.ListAsync(Paging.Skip(current, size), size, cancellationToken))
            servers.Add(await adminViewService.StatusAsync(server, cancellationToken));

        return View(new ServersViewModel
        {
            Servers = new PagedListModel<ServerStatusViewModel>(servers, current, size, total),
            ReturnUrl = AdminListUrls.TabPage(AppConsts.AdminServersUrl, AppConsts.ServersListTab, AppConsts.AdminPageQuery, current)
        });
    }

    [HttpPost("servers/live")]
    public async Task<IActionResult> LiveSettings(LiveServerSettings settings, CancellationToken cancellationToken)
    {
        settings.Normalize();

        await settingsService.SaveAsync(settings, cancellationToken);
        liveSettings.Set(settings);
        TempData["LiveSaved"] = true;

        return Redirect(AdminTabs.Url(AppConsts.AdminServersUrl, AppConsts.ServersLiveTab));
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
        await gameServerService.SetAlternativeAddressAsync(credentials.Server.Id, form.AlternativeAddress, cancellationToken);
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
    [AllowDuringSetup]
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
        {
            await gameServerService.UpdateAsync(id, form.Name, form.Address, form.Enabled, cancellationToken);
            await gameServerService.SetAlternativeAddressAsync(id, form.AlternativeAddress, cancellationToken);
        }

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
    public async Task<IActionResult> DeleteServer(int id, string? returnUrl, CancellationToken cancellationToken)
    {
        if (!await gameServerService.DeleteAsync(id, cancellationToken))
            TempData["Error"] = "A server that already sent statistics cannot be deleted. Disable it instead.";

        return Back(returnUrl, AppConsts.AdminServersUrl);
    }

    [HttpGet("settings")]
    public async Task<IActionResult> Settings(CancellationToken cancellationToken)
    {
        return View(await adminViewService.SettingsAsync(cancellationToken));
    }

    [HttpPost("settings")]
    [RequestSizeLimit(AppConsts.SettingsFormMaxBytes)]
    public async Task<IActionResult> SaveSettings(SettingsViewModel model, string? tab, CancellationToken cancellationToken)
    {
        var problems = await adminViewService.SaveAsync(model, cancellationToken);

        TempData["Saved"] = true;

        if (problems.Count > 0)
            TempData[AppConsts.SettingsProblemsTempData] = problems.ToArray();

        return Redirect(AdminTabs.Url(AppConsts.AdminSettingsUrl, AdminTabs.Active(tab, AppConsts.SettingsSiteTab, AppConsts.SettingsAppearanceTab, AppConsts.SettingsSeasonsTab, AppConsts.SettingsRankingTab, AppConsts.SettingsPrivacyTab)));
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
        await skinCatalogService.SeedAsync(cancellationToken);
        TempData["Saved"] = true;

        return RedirectToAction(nameof(Modules));
    }

    [HttpGet("game-modes")]
    public async Task<IActionResult> GameModes(int page = 1, int seenPage = 1, CancellationToken cancellationToken = default)
    {
        var rulesSize = AppConsts.GameModeRulesPageSize;
        var rulesTotal = await gameModeService.CountRulesAsync(cancellationToken);
        var rulesPage = Paging.Page(page, rulesTotal, rulesSize);
        var rules = await gameModeService.ListRulesAsync(Paging.Skip(rulesPage, rulesSize), rulesSize, cancellationToken);

        var seenSize = AppConsts.GameConfigurationsPageSize;
        var seenTotal = await gameModeService.CountConfigurationsAsync(cancellationToken);
        var seenCurrent = Paging.Page(seenPage, seenTotal, seenSize);
        var configurations = await gameModeService.ListConfigurationsAsync(Paging.Skip(seenCurrent, seenSize), seenSize, cancellationToken);
        var ruleSet = await gameModeService.GetRuleSetAsync(cancellationToken);
        var seen = configurations.Select(c => new SeenConfiguration(c, ruleSet.Find(c.Name, c.TeamSize))).ToList();

        var model = new GameModesViewModel
        {
            Rules = new PagedListModel<GameModeRule>(rules, rulesPage, rulesSize, rulesTotal),
            Configurations = new PagedListModel<SeenConfiguration>(seen, seenCurrent, seenSize, seenTotal),
            ReturnUrl = AdminListUrls.TabPage(AppConsts.AdminGameModesUrl, AppConsts.GameModesRulesTab, AppConsts.AdminPageQuery, rulesPage)
        };

        return View(model);
    }

    [HttpPost("game-modes")]
    public async Task<IActionResult> SaveGameMode(GameModeRule rule, string? returnUrl, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(rule.Name))
            await gameModeService.SaveRuleAsync(rule, cancellationToken);

        return Back(returnUrl, AppConsts.AdminGameModesUrl);
    }

    [HttpPost("game-modes/{id:int}/delete")]
    public async Task<IActionResult> DeleteGameMode(int id, string? returnUrl, CancellationToken cancellationToken)
    {
        await gameModeService.DeleteRuleAsync(id, cancellationToken);

        return Back(returnUrl, AppConsts.AdminGameModesUrl);
    }

    [HttpGet("campaigns")]
    public async Task<IActionResult> Campaigns(int page = 1, CancellationToken cancellationToken = default)
    {
        var size = AppConsts.CampaignsPageSize;
        var total = await campaignService.CountAsync(cancellationToken);
        var current = Paging.Page(page, total, size);
        var campaigns = await campaignService.ListAsync(Paging.Skip(current, size), size, cancellationToken);

        return View(new CampaignsViewModel
        {
            Campaigns = new PagedListModel<Campaign>(campaigns, current, size, total),
            ReturnUrl = AdminListUrls.Page(AppConsts.AdminCampaignsUrl, current)
        });
    }

    [HttpPost("campaigns")]
    public async Task<IActionResult> SaveCampaign(CampaignForm form, string? returnUrl, CancellationToken cancellationToken)
    {
        var maps = (form.Maps ?? string.Empty).Split(['\n', '\r', ',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (!string.IsNullOrWhiteSpace(form.Name) && maps.Length > 0)
            await campaignService.SaveAsync(form.Id, form.Name, maps, cancellationToken);

        return Back(returnUrl, AppConsts.AdminCampaignsUrl);
    }

    [HttpPost("campaigns/{id:int}/delete")]
    public async Task<IActionResult> DeleteCampaign(int id, string? returnUrl, CancellationToken cancellationToken)
    {
        await campaignService.DeleteAsync(id, cancellationToken);

        return Back(returnUrl, AppConsts.AdminCampaignsUrl);
    }

    [HttpGet("administrators")]
    public async Task<IActionResult> Administrators(int page = 1, CancellationToken cancellationToken = default)
    {
        var size = AppConsts.AdministratorsPageSize;
        var total = await administratorService.CountAsync(cancellationToken);
        var current = Paging.Page(page, total, size);
        var administrators = await administratorService.ListAsync(Paging.Skip(current, size), size, cancellationToken);

        return View(new AdministratorsViewModel
        {
            Administrators = new PagedListModel<Administrator>(administrators, current, size, total),
            Current = currentUser.CommunityId,
            ReturnUrl = AdminListUrls.Page(AppConsts.AdminAdministratorsUrl, current)
        });
    }

    [HttpPost("administrators")]
    public async Task<IActionResult> AddAdministrator(string? communityId, string? name, string? returnUrl, CancellationToken cancellationToken)
    {
        var parsed = SteamIdParser.Parse(communityId);

        if (parsed != null)
            await administratorService.AddAsync(parsed.Value, name, cancellationToken);
        else
            TempData["Error"] = "That is not a valid SteamID64.";

        return Back(returnUrl, AppConsts.AdminAdministratorsUrl);
    }

    [HttpPost("administrators/{communityId:long}/delete")]
    public async Task<IActionResult> RemoveAdministrator(long communityId, string? returnUrl, CancellationToken cancellationToken)
    {
        if (communityId == currentUser.CommunityId || !await administratorService.RemoveAsync(communityId, cancellationToken))
            TempData["Error"] = "You cannot remove yourself or the last administrator.";

        return Back(returnUrl, AppConsts.AdminAdministratorsUrl);
    }

    [HttpPost("reprocess")]
    public async Task<IActionResult> Reprocess(CancellationToken cancellationToken)
    {
        TempData["Reprocessed"] = await matchBuildService.RebuildAllAsync(cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    private IActionResult Back(string? returnUrl, string fallback)
    {
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : Redirect(fallback);
    }
}
