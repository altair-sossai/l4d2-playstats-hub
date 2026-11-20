using L4D2PlayStats.Hub.Core.Modules.Servers;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Extensions;

public static class GameServerHttpContextExtensions
{
    public static GameServer GameServer(this HttpContext httpContext)
    {
        return (GameServer)httpContext.Items[AppConsts.GameServerItem]!;
    }
}
