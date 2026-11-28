using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Models;

public class ChampionshipSettingsForm
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? StreamUrl { get; set; }
    public string? VideosUrl { get; set; }
    public string? GameConfiguration { get; set; }
    public int PlayersPerGame { get; set; } = AppConsts.ChampionshipDefaultPlayersPerGame;
    public int RosterSize { get; set; } = AppConsts.ChampionshipDefaultPlayersPerGame;
    public int? MaxParticipants { get; set; }
    public bool Substitutes { get; set; }
    public bool JoinRequests { get; set; }
    public ChampionshipDraftMethod DraftMethod { get; set; }
    public bool AutoLinkMatches { get; set; } = true;
    public int AutoLinkHours { get; set; } = AppConsts.ChampionshipDefaultAutoLinkHours;
    public int AutoCloseMinutes { get; set; } = AppConsts.ChampionshipDefaultAutoCloseMinutes;
    public bool AntiCheatRequired { get; set; }
    public DayOfWeek? SuggestedWeekday { get; set; }
    public int? SuggestedMinute { get; set; }
    public List<int> CampaignIds { get; set; } = [];
    public DateTime? StartsOn { get; set; }
    public DateTime? EndsOn { get; set; }
}
