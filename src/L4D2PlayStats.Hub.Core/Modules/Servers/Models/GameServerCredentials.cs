namespace L4D2PlayStats.Hub.Core.Modules.Servers.Models;

public record GameServerCredentials(GameServer Server, string Key)
{
    public string Token => $"{Server.PublicId}{AppConsts.GameServerTokenSeparator}{Key}";
}
