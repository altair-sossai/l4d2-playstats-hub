using L4D2PlayStats.Hub.Web.Infrastructure.Localization;
using L4D2PlayStats.Hub.Web.Models.Api;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Invalid(string message)
    {
        var localizer = HttpContext.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();

        return BadRequest(new ApiErrorResult(localizer[message].Value));
    }

    protected async Task<IActionResult?> InvalidAsync<T>(T command, CancellationToken cancellationToken)
    {
        var error = await HttpContext.RequestServices.GetRequiredService<IValidationService>().ErrorAsync(command, cancellationToken);

        return error == null ? null : Invalid(error);
    }
}
