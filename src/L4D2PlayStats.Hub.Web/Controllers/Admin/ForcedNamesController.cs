using L4D2PlayStats.Hub.Core.Modules.Players.Enums;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Models.Players;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[Route("admin/forced-names")]
public class ForcedNamesController(IForcedNameService forcedNameService, IRelatedAccountsService relatedAccountsService, ICurrentUser currentUser) : AdminControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        var names = await forcedNameService.PageAsync(PlayerSearch.Of(q), page, AppConsts.ForcedNamesPageSize, cancellationToken);

        return View(new ForcedNamesViewModel
        {
            Names = new PagedListModel<ForcedNameEntry>(names),
            Search = q?.Trim(),
            ReturnUrl = ForcedNameUrls.Page(q, names.Number)
        });
    }

    [HttpPost("save")]
    public async Task<IActionResult> Save(ForcedNameForm form, CancellationToken cancellationToken)
    {
        if (!await ValidAsync(form, cancellationToken))
            return Back(form.ReturnUrl, AppConsts.ForcedNamesUrl);

        var communityId = SteamIdParser.Parse(form.CommunityId)!.Value;
        var result = await forcedNameService.SaveAsync(communityId, form.Name, currentUser.CommunityId, cancellationToken);

        if (result == ForcedNameSaveResult.Saved)
        {
            var related = 0;

            foreach (var relatedId in await relatedAccountsService.ValidAsync(communityId, form.RelatedIds, cancellationToken))
                if (await forcedNameService.SaveAsync(relatedId, form.Name, currentUser.CommunityId, cancellationToken) == ForcedNameSaveResult.Saved)
                    related++;

            if (related > 0)
                TempData[AppConsts.RelatedLinkedTempData] = related;

            Saved();
        }
        else
            Error(result == ForcedNameSaveResult.InvalidPlayer ? AppConsts.InvalidSteamIdMessage : AppConsts.ForcedNameInvalidNameMessage);

        return Back(form.ReturnUrl, AppConsts.ForcedNamesUrl);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(PlayerForm form, CancellationToken cancellationToken)
    {
        if (await ValidAsync(form, cancellationToken) && await forcedNameService.DeleteAsync(SteamIdParser.Parse(form.CommunityId)!.Value, cancellationToken))
            TempData[AppConsts.ForcedNameRemovedTempData] = true;

        return Back(form.ReturnUrl, AppConsts.ForcedNamesUrl);
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import(string? text, CancellationToken cancellationToken)
    {
        var import = await forcedNameService.ImportAsync(text, currentUser.CommunityId, cancellationToken);

        TempData[AppConsts.ForcedNamesImportedTempData] = new[] { import.Added, import.Updated, import.Unchanged, import.Invalid.Count };

        return Redirect(AppConsts.ForcedNamesUrl);
    }
}
