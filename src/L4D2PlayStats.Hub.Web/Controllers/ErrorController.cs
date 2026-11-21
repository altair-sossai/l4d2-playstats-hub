using L4D2PlayStats.Hub.Web.Infrastructure.Errors;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace L4D2PlayStats.Hub.Web.Controllers;

[AllowDuringSetup]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController(ILogger<ErrorController> logger) : Controller
{
    [Route(AppConsts.ErrorPath)]
    [Route($"{AppConsts.ErrorPath}/{{statusCode:int}}")]
    public IActionResult Index(int? statusCode = null)
    {
        var code = statusCode is >= StatusCodes.Status400BadRequest and < AppConsts.ErrorStatusCodeLimit ? statusCode.Value : StatusCodes.Status500InternalServerError;
        var exception = HttpContext.Features.Get<IExceptionHandlerFeature>();

        if (exception != null && !ErrorPages.IsExceptionLogged(HttpContext))
            logger.LogError(exception.Error, "Unhandled exception during request: {Method} {Path}", Request.Method, exception.Path);

        Response.StatusCode = code;

        if (ErrorPages.IsInGame(HttpContext))
            return Content($"{code} {ReasonPhrases.GetReasonPhrase(code)}", AppConsts.PlainTextContentType);

        if (!ErrorPages.ShowsPage(HttpContext))
            return new EmptyResult();

        return View(new ErrorViewModel(code));
    }
}
