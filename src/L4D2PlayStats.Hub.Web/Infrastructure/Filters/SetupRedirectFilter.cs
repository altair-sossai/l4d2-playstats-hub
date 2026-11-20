using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using L4D2PlayStats.Hub.Core.Modules.Administrators.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Filters;

public class SetupRedirectFilter(ISetupService setupService) : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var path = context.HttpContext.Request.Path;

        var exempt = path.StartsWithSegments("/setup")
                     || path.StartsWithSegments("/api")
                     || path.StartsWithSegments("/login")
                     || path.StartsWithSegments("/logout")
                     || path.StartsWithSegments("/language")
                     || path.StartsWithSegments(AppConsts.LogoUrl)
                     || path.StartsWithSegments(AppConsts.ThemeSwitchPath)
                     || path.StartsWithSegments("/dev");

        exempt = exempt || context.ActionDescriptor.EndpointMetadata.OfType<AllowDuringSetupAttribute>().Any();

        if (!exempt && await setupService.IsPendingAsync(context.HttpContext.RequestAborted))
        {
            context.Result = new RedirectResult("/setup");
            return;
        }

        await next();
    }
}
