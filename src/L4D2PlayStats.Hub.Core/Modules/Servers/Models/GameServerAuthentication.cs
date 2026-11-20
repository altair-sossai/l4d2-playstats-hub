using L4D2PlayStats.Hub.Core.Modules.Servers.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Servers.Models;

public record GameServerAuthentication(GameServerAuthenticationStatus Status, GameServer? Server)
{
    public bool Succeeded => Status == GameServerAuthenticationStatus.Success && Server != null;
}