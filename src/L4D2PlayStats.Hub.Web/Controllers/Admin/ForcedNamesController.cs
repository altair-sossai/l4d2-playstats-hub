using L4D2PlayStats.Hub.Core.Modules.Players.Enums;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Models.Players;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[Authorize(Policy = AppConsts.AdministratorPolicy)]
[AutoValidateAntiforgeryToken]
[Route("admin/forced-names")]
public class ForcedNamesController(IForcedNameService forcedNameService, ICurrentUser currentUser) : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewBag.Admin = "active";
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        var size = AppConsts.ForcedNamesPageSize;
        var total = await forcedNameService.CountAsync(q, cancellationToken);
        var current = Paging.Page(page, total, size);
        var names = await forcedNameService.ListAsync(q, Paging.Skip(current, size), size, cancellationToken);

        return View(new ForcedNamesViewModel
        {
            Names = new PagedListModel<ForcedNameEntry>(names, current, size, total),
            Search = q?.Trim(),
            ReturnUrl = ForcedNameUrls.Page(q, current)
        });
    }

    [HttpPost("save")]
    public async Task<IActionResult> Save(string? communityId, string? name, string? returnUrl, CancellationToken cancellationToken)
    {
        var parsed = SteamIdParser.Parse(communityId);

        var result = parsed == null
            ? ForcedNameSaveResult.InvalidPlayer
            : await forcedNameService.SaveAsync(parsed.Value, name, currentUser.CommunityId, cancellationToken);

        if (result == ForcedNameSaveResult.Saved)
            TempData["Saved"] = true;
        else
            TempData["Error"] = result == ForcedNameSaveResult.InvalidPlayer ? AppConsts.ForcedNameInvalidPlayerMessage : AppConsts.ForcedNameInvalidNameMessage;

        return Back(returnUrl);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(string? communityId, string? returnUrl, CancellationToken cancellationToken)
    {
        var parsed = SteamIdParser.Parse(communityId);

        if (parsed != null && await forcedNameService.DeleteAsync(parsed.Value, cancellationToken))
            TempData["ForcedNameRemoved"] = true;

        return Back(returnUrl);
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import(string? text, CancellationToken cancellationToken)
    {
        var import = await forcedNameService.ImportAsync(text, currentUser.CommunityId, cancellationToken);

        TempData["ForcedNamesImported"] = new[] { import.Added, import.Updated, import.Unchanged, import.Invalid.Count };

        return Redirect(AppConsts.ForcedNamesUrl);
    }

    private IActionResult Back(string? returnUrl)
    {
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : Redirect(AppConsts.ForcedNamesUrl);
    }
}
