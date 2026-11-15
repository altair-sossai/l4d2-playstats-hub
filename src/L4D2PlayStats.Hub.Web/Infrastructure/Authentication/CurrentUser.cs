using L4D2PlayStats.Hub.Core.Modules.Administrators.Services;
using System.Security.Claims;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Authentication;

public class CurrentUser(IHttpContextAccessor httpContextAccessor, IAdministratorService administratorService) : ICurrentUser
{
    private bool? _administrator;

    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => CommunityId != null;

    public long? CommunityId => SteamClaims.CommunityId(Principal);

    public string? Name => Principal?.FindFirstValue(ClaimTypes.Name);

    public bool ItIsMe(long? communityId)
    {
        return communityId != null && CommunityId == communityId;
    }

    public bool ItIsMe(string? communityId)
    {
        return long.TryParse(communityId, out var parsed) && ItIsMe(parsed);
    }

    public async Task<bool> IsAdministratorAsync()
    {
        if (_administrator != null)
            return _administrator.Value;

        var communityId = CommunityId;
        _administrator = communityId != null && await administratorService.IsAdministratorAsync(communityId.Value);

        return _administrator.Value;
    }
}
