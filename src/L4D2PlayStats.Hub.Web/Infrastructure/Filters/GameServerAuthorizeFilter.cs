using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Filters;

public class GameServerAuthorizeFilter(IGameServerService gameServerService) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var token = context.HttpContext.Request.Headers[AppConsts.AuthorizationHeader].ToString();
        var authentication = await gameServerService.AuthenticateAsync(token, context.HttpContext.RequestAborted);

        if (!authentication.Succeeded)
        {
            context.Result = new ObjectResult(new { error = authentication.Status.ToString() }) { StatusCode = StatusCodes.Status401Unauthorized };
            return;
        }

        context.HttpContext.Items[AppConsts.GameServerItem] = authentication.Server;
    }
}
