using L4D2PlayStats.Hub.Core.Modules.Notifications.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
[Route(AppConsts.NotificationsUrl)]
public class NotificationsController(INotificationService notifications, INotificationViewService viewService, ICurrentUser currentUser) : HubControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default)
    {
        var model = await viewService.PageAsync(page, cancellationToken);

        if (model == null)
            return Challenge();

        return View(model);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Open(int id, CancellationToken cancellationToken)
    {
        if (currentUser.CommunityId is not { } me)
            return Challenge();

        var notification = await notifications.OpenAsync(me, await currentUser.IsAdministratorAsync(), id, cancellationToken);

        if (notification == null)
            return NotFound();

        return Back(notification.Url, AppConsts.NotificationsUrl);
    }

    [HttpPost("read")]
    public async Task<IActionResult> ReadAll(string? returnUrl, CancellationToken cancellationToken)
    {
        if (currentUser.CommunityId is not { } me)
            return Challenge();

        await notifications.ReadAllAsync(me, await currentUser.IsAdministratorAsync(), cancellationToken);
        Saved();

        return Back(returnUrl, AppConsts.NotificationsUrl);
    }
}
