using L4D2PlayStats.Hub.Core.Modules.Administrators;
using L4D2PlayStats.Hub.Core.Modules.Campaigns;
using L4D2PlayStats.Hub.Core.Modules.GameModes;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Settings;

namespace L4D2PlayStats.Hub.Web.Models;

public class DashboardViewModel
{
    public IReadOnlyList<ServerStatusViewModel> Servers { get; init; } = [];
    public int Files { get; init; }
    public int Matches { get; init; }
    public int RankedMatches { get; init; }
    public int Players { get; init; }
    public int Seasons { get; init; }
    public int UnmatchedConfigurations { get; init; }
}

public class ServerStatusViewModel
{
    public GameServer Server { get; init; } = null!;
    public GameServerStatus Status { get; init; } = null!;
    public ModuleSettings Modules { get; init; } = null!;
    public int Files { get; init; }
}

public class ServerViewModel
{
    public ServerStatusViewModel Status { get; init; } = null!;
    public string? Token { get; init; }
    public string SiteUrl { get; init; } = null!;
    public bool Wizard { get; init; }
}

public class ServerForm
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public bool Enabled { get; set; } = true;
}

public class SettingsViewModel
{
    public SiteSettings Site { get; set; } = new();
    public SeasonSettings Season { get; set; } = new();
    public RankingSettings Ranking { get; set; } = new();
    public IReadOnlyList<string> TimeZones { get; set; } = [];
}

public class GameModesViewModel
{
    public IReadOnlyList<GameModeRule> Rules { get; init; } = [];
    public IReadOnlyList<SeenConfiguration> Configurations { get; init; } = [];
}

public record SeenConfiguration(GameConfiguration Configuration, GameModeRule? Rule);

public class CampaignForm
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? Maps { get; set; }
}

public class CampaignsViewModel
{
    public IReadOnlyList<Campaign> Campaigns { get; init; } = [];
}

public class AdministratorsViewModel
{
    public IReadOnlyList<Administrator> Administrators { get; init; } = [];
    public long? Current { get; init; }
}

public class SetupViewModel
{
    public string Step { get; init; } = null!;
    public string? Token { get; init; }
    public string? Error { get; init; }
    public bool DevelopmentSignIn { get; init; }
    public SettingsViewModel Settings { get; init; } = new();
    public ModuleSettings Modules { get; init; } = new();
    public ServerViewModel? Server { get; init; }
}
