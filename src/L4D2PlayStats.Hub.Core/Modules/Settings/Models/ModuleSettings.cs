using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Settings.Models;

public class ModuleSettings
{
    public bool Ranking { get; set; } = true;
    public bool LiveServer { get; set; } = true;
    public bool SkinShop { get; set; }
    public bool CommunityRanking { get; set; }
    public bool SiteChat { get; set; }
    public bool RemoteCommands { get; set; }
    public bool AntiCheat { get; set; }
    public bool Championships { get; set; }
    public bool Bans { get; set; }

    public bool IsEnabled(HubModule module)
    {
        return module switch
        {
            HubModule.Ranking => Ranking,
            HubModule.LiveServer => LiveServer,
            HubModule.SkinShop => SkinShop && Ranking,
            HubModule.CommunityRanking => CommunityRanking,
            HubModule.SiteChat => SiteChat && LiveServer,
            HubModule.RemoteCommands => RemoteCommands && LiveServer,
            HubModule.AntiCheat => AntiCheat,
            HubModule.Championships => Championships,
            HubModule.Bans => Bans,
            _ => false
        };
    }
}
