using L4D2PlayStats.Hub.Core.Modules.Settings.Models;

namespace L4D2PlayStats.Hub.Web.Models;

public class SettingsViewModel
{
    public SiteSettings Site { get; set; } = new();
    public SeasonSettings Season { get; set; } = new();
    public RankingSettings Ranking { get; set; } = new();
    public IReadOnlyList<string> TimeZones { get; set; } = [];
}
