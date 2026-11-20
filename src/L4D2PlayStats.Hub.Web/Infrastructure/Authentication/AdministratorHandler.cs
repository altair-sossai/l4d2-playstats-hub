using Microsoft.AspNetCore.Authorization;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Authentication;

public class AdministratorHandler(ICurrentUser currentUser) : AuthorizationHandler<AdministratorRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdministratorRequirement requirement)
    {
        if (await currentUser.IsAdministratorAsync())
            context.Succeed(requirement);
    }
}
