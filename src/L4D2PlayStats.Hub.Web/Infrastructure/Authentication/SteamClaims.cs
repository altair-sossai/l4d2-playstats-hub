using System.Security.Claims;
using System.Text.RegularExpressions;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Authentication;

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
