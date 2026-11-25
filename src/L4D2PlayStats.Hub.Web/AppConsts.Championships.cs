namespace L4D2PlayStats.Hub.Web;

public static partial class AppConsts
{
    public const string ChampionshipsUrl = "/championships";
    public const string AdminChampionshipsUrl = "/admin/championships";
    public const string ChampionshipLocalDateFormat = "yyyy-MM-ddTHH:mm";
    public static readonly string[] ChampionshipLocalDateFormats = [ChampionshipLocalDateFormat, "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.fff"];
    public const string ChampionshipDateOnlyFormat = "yyyy-MM-dd";
    public const string ChampionshipCalendarDateFormat = "yyyyMMdd'T'HHmmss'Z'";
    public const string ChampionshipCalendarContentType = "text/calendar";
    public const string ChampionshipGoogleCalendarUrl = "https://calendar.google.com/calendar/render?action=TEMPLATE";
    public const int ChampionshipCalendarDurationInMinutes = 90;
    public const int ChampionshipUpcomingSeries = 6;
    public const string ChampionshipOverviewTab = "overview";
    public const string ChampionshipRulesTab = "rules";
    public const string ChampionshipTeamsTab = "teams";
    public const string ChampionshipDraftTab = "draft";
    public const string ChampionshipStandingsTab = "standings";
    public const string ChampionshipBracketTab = "bracket";
    public const string ChampionshipGamesTab = "games";
    public const string ChampionshipStatisticsTab = "statistics";
    public const string ChampionshipAdminSettingsTab = "settings";
    public const string ChampionshipAdminParticipantsTab = "participants";
    public const string ChampionshipAdminTeamsTab = "teams";
    public const string ChampionshipAdminDraftTab = "draft";
    public const string ChampionshipAdminStagesTab = "stages";
    public const string ChampionshipAdminGamesTab = "games";
    public const string ChampionshipAdminHistoryTab = "history";
    public const string ChampionshipParticipantsPageQuery = "participantsPage";
    public const string ChampionshipGamesPageQuery = "gamesPage";
    public const string ChampionshipHistoryPageQuery = "historyPage";
    public const string ChampionshipStageQuery = "stage";
    public const string ChampionshipMatchResult = "match";
    public const string ChampionshipTypedResult = "typed";
    public const string ChampionshipNoResult = "none";
    public const string ChampionshipClearCampaigns = "clear";
    public const string ChampionshipDrawCampaigns = "draw";
    public const int ChampionshipsPageSize = 20;
    public const int ChampionshipParticipantsPageSize = 20;
    public const int ChampionshipGamesPageSize = 20;
    public const int ChampionshipHistoryPageSize = 20;
    public const string ChampionshipPortugueseCulture = "pt";
    public const string ChampionshipSpanishCulture = "es";
}
