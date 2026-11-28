using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Bans;
using L4D2PlayStats.Hub.Core.Modules.Bans.Enums;
using L4D2PlayStats.Hub.Core.Modules.Bans.Models;
using L4D2PlayStats.Hub.Core.Modules.Bans.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Models.Bans;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[Route("admin/bans")]
[RequireModule(HubModule.Bans)]
public class BansAdminController(
    IBanService banService,
    IBanAlertService alertService,
    IRelatedAccountsService relatedAccountsService,
    ISiteClock siteClock,
    IClock clock,
    ICurrentUser currentUser) : AdminControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, string? tab, int page = 1, int endedPage = 1, int alertsPage = 1, CancellationToken cancellationToken = default)
    {
        var search = PlayerSearch.Of(q);
        var active = await banService.PageAsync(search, BanListFilter.Active, page, AppConsts.BansPageSize, cancellationToken);
        var ended = await banService.PageAsync(search, BanListFilter.Ended, endedPage, AppConsts.BansPageSize, cancellationToken);
        var alerts = await alertService.PageAsync(alertsPage, AppConsts.BansPageSize, cancellationToken);
        var selected = AdminTabs.Active(tab, AppConsts.BansActiveTab, AppConsts.BansEndedTab, AppConsts.BansAlertsTab);
        var term = q?.Trim();

        return View(new BansAdminModel
        {
            Active = new PagedListModel<Ban>(active),
            Ended = new PagedListModel<Ban>(ended),
            Alerts = new PagedListModel<BanAlertEntry>(alerts),
            ActiveCount = await banService.CountAsync(BanListFilter.Active, cancellationToken),
            Tab = selected,
            Search = term,
            ReturnUrl = BanUrls.Admin(term, AppConsts.BansActiveTab, active.Number),
            AlertsReturnUrl = BanUrls.Admin(term, AppConsts.BansAlertsTab, alerts.Number),
            TimeZone = await siteClock.ZoneAsync(cancellationToken)
        });
    }

    [HttpPost("save")]
    public async Task<IActionResult> Save(BanForm form, CancellationToken cancellationToken)
    {
        if (!await ValidAsync(form, cancellationToken))
            return Back(form.ReturnUrl, AppConsts.BansAdminUrl);

        var timeZone = await siteClock.ZoneAsync(cancellationToken);

        if (!BanDurations.TryEndsAt(form.Duration, form.EndsAt, timeZone, clock.UtcNow, out var endsAt))
        {
            Error(AppConsts.BanInvalidEndMessage);
            return Back(form.ReturnUrl, AppConsts.BansAdminUrl);
        }

        var communityId = SteamIdParser.Parse(form.CommunityId)!.Value;
        var result = await banService.AddAsync(new BanRequest(communityId, form.Name, endsAt, form.Reason, form.RelatedToBanId), Actor(), cancellationToken);

        if (result != BanSaveResult.Saved)
        {
            Error(Message(result));
            return Back(form.ReturnUrl, AppConsts.BansAdminUrl);
        }

        var main = await banService.ActiveAsync(communityId, cancellationToken);
        var related = 0;

        foreach (var relatedId in await relatedAccountsService.ValidAsync(communityId, form.RelatedIds, cancellationToken))
            if (await banService.AddAsync(new BanRequest(relatedId, null, endsAt, form.Reason, main?.Id), Actor(), cancellationToken) == BanSaveResult.Saved)
                related++;

        if (related > 0)
            TempData[AppConsts.RelatedLinkedTempData] = related;

        TempData[AppConsts.BanSavedTempData] = true;

        return Back(form.ReturnUrl, AppConsts.BansAdminUrl);
    }

    [HttpPost("edit")]
    public async Task<IActionResult> Edit(BanEditForm form, CancellationToken cancellationToken)
    {
        if (!await ValidAsync(form, cancellationToken))
            return Back(form.ReturnUrl, AppConsts.BansAdminUrl);

        var timeZone = await siteClock.ZoneAsync(cancellationToken);

        if (!BanDurations.TryEndsAt(form.Duration, form.EndsAt, timeZone, clock.UtcNow, out var endsAt))
        {
            Error(AppConsts.BanInvalidEndMessage);
            return Back(form.ReturnUrl, AppConsts.BansAdminUrl);
        }

        var result = await banService.EditAsync(form.Id, endsAt, form.Reason, Actor(), cancellationToken);

        if (result == BanSaveResult.Saved)
            TempData[AppConsts.BanSavedTempData] = true;
        else
            Error(Message(result));

        return Back(form.ReturnUrl, AppConsts.BansAdminUrl);
    }

    [HttpPost("lift")]
    public async Task<IActionResult> Lift(BanLiftForm form, CancellationToken cancellationToken)
    {
        if (await banService.LiftAsync(form.Id, form.Reason, Actor(), cancellationToken))
            TempData[AppConsts.BanLiftedTempData] = true;
        else
            Error(AppConsts.BanNotFoundMessage);

        return Back(form.ReturnUrl, AppConsts.BansAdminUrl);
    }

    [HttpPost("alerts/ignore")]
    public async Task<IActionResult> Ignore(BanAlertForm form, CancellationToken cancellationToken)
    {
        if (await alertService.IgnoreAsync(form.Id, currentUser.CommunityId, cancellationToken))
            TempData[AppConsts.BanAlertIgnoredTempData] = true;
        else
            Error(AppConsts.BanAlertNotFoundMessage);

        return Back(form.ReturnUrl, AdminTabs.Url(AppConsts.BansAdminUrl, AppConsts.BansAlertsTab));
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import(string? text, CancellationToken cancellationToken)
    {
        var import = await banService.ImportAsync(text, Actor(), cancellationToken);

        TempData[AppConsts.BansImportedTempData] = new[] { import.Added, import.Unchanged, import.Invalid.Count };

        return Redirect(AppConsts.BansAdminUrl);
    }

    private BanActor Actor()
    {
        return new BanActor(currentUser.CommunityId, currentUser.Name);
    }

    private static string Message(BanSaveResult result)
    {
        return result switch
        {
            BanSaveResult.InvalidPlayer => AppConsts.InvalidSteamIdMessage,
            BanSaveResult.InvalidEnd => AppConsts.BanInvalidEndMessage,
            BanSaveResult.AlreadyBanned => AppConsts.BanAlreadyActiveMessage,
            _ => AppConsts.BanNotFoundMessage
        };
    }
}
