using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Filters;

public class RequireModuleFilter(ISettingsService settingsService) : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(context.HttpContext.RequestAborted);

        var required = context.ActionDescriptor.EndpointMetadata
            .OfType<RequireModuleAttribute>()
            .Select(attribute => attribute.Module);

        if (required.Any(module => !modules.IsEnabled(module)))
        {
            context.Result = new NotFoundResult();
            return;
        }

        await next();
    }
}
