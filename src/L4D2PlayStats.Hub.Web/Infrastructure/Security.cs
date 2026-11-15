using System.Security.Claims;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Modules.Administrators;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace L4D2PlayStats.Hub.Web.Infrastructure;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    long? CommunityId { get; }
    string? Name { get; }
    Task<bool> IsAdministratorAsync();
}

public class CurrentUser(IHttpContextAccessor httpContextAccessor, IAdministratorService administratorService) : ICurrentUser
{
    private bool? _administrator;

    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => CommunityId != null;

    public long? CommunityId => SteamClaims.CommunityId(Principal);

    public string? Name => Principal?.FindFirstValue(ClaimTypes.Name);

    public async Task<bool> IsAdministratorAsync()
    {
        if (_administrator != null)
            return _administrator.Value;

        var communityId = CommunityId;
        _administrator = communityId != null && await administratorService.IsAdministratorAsync(communityId.Value);

        return _administrator.Value;
    }
}

public static class SteamClaims
{
    public static long? CommunityId(ClaimsPrincipal? principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
            return null;

        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(value))
            return null;

        if (long.TryParse(value, out var direct))
            return direct;

        var match = Regex.Match(value, AppConsts.SteamOpenIdPattern);

        return match.Success && long.TryParse(match.Groups[1].Value, out var communityId) ? communityId : null;
    }
}

public class AdministratorRequirement : IAuthorizationRequirement;

public class AdministratorHandler(ICurrentUser currentUser) : AuthorizationHandler<AdministratorRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdministratorRequirement requirement)
    {
        if (await currentUser.IsAdministratorAsync())
            context.Succeed(requirement);
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class GameServerAuthorizeAttribute() : TypeFilterAttribute(typeof(GameServerAuthorizeFilter));

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

public static class GameServerHttpContextExtensions
{
    public static GameServer GameServer(this HttpContext httpContext)
    {
        return (GameServer)httpContext.Items[AppConsts.GameServerItem]!;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireModuleAttribute(HubModule module) : TypeFilterAttribute(typeof(RequireModuleFilter))
{
    public HubModule Module { get; } = module;
}

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
                     || path.StartsWithSegments("/dev");

        if (!exempt && await setupService.IsPendingAsync(context.HttpContext.RequestAborted))
        {
            context.Result = new RedirectResult("/setup");
            return;
        }

        await next();
    }
}
