namespace L4D2PlayStats.Hub.Core.Modules.Settings;

public class Setting
{
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
}

public class SiteSettings
{
    public string Name { get; set; } = AppConsts.DefaultSiteName;
    public string TimeZone { get; set; } = AppConsts.DefaultTimeZone;
    public string DefaultCulture { get; set; } = AppConsts.DefaultCulture;
    public string? PublicUrl { get; set; }
    public string? SteamApiKey { get; set; }
}

public class SetupSettings
{
    public string? Token { get; set; }
    public bool Completed { get; set; }
}

public class ModuleSettings
{
    public bool Ranking { get; set; } = true;
    public bool LiveServer { get; set; } = true;
    public bool SkinShop { get; set; }
    public bool CommunityRanking { get; set; }
    public bool SiteChat { get; set; }
    public bool RemoteCommands { get; set; }
    public bool AntiCheat { get; set; }

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
            _ => false
        };
    }
}

public enum HubModule
{
    Ranking,
    LiveServer,
    SkinShop,
    CommunityRanking,
    SiteChat,
    RemoteCommands,
    AntiCheat
}

public class SeasonSettings
{
    public int LengthInMonths { get; set; } = AppConsts.DefaultSeasonLengthInMonths;
}

public class RankingSettings
{
    public int Win { get; set; } = AppConsts.DefaultWinExperience;
    public int Loss { get; set; } = AppConsts.DefaultLossExperience;
    public int RageQuit { get; set; } = AppConsts.DefaultRageQuitExperience;
    public int Mvp { get; set; } = AppConsts.DefaultMvpExperience;
    public int MvpCommon { get; set; } = AppConsts.DefaultMvpCommonExperience;
    public int MinimumMatches { get; set; }
    public bool IncludeUnknownCampaigns { get; set; } = true;
}
