namespace L4D2PlayStats.Hub.Web;

public static partial class AppConsts
{
    public const int LivePageRefreshInMilliseconds = 2600;
    public const int LivePageRequestTimeoutInMilliseconds = 2000;
    public const int LivePageMaxFeedItems = 300;
    public const int ServerPickerRefreshInMilliseconds = 10000;
    public const int ChatBansPageSize = 20;
    public const string AntiCheatUrl = "/anti-cheat";
    public const string NoCampaignImage = "/imgs/campaigns/no-image.jpg";
    public const string ServersUrl = "/servers";
    public const string ServersApiUrl = "/api/servers";
    public const string ChatBansUrl = "/admin/chat-bans";
    public const string PlayerUrl = "/player/";
    public const string AvatarUrl = "/avatar/";
    public const string CopyIconLight = "copy-light.png";
    public const string CopyIconDark = "copy-dark.png";
    public const string IconThemeLight = "light";
    public const string IconThemeDark = "dark";
    public const string FeedKindMessage = "message";
    public const string FeedKindEvent = "event";
    public const string FeedMessagePartial = "_Message";
    public const string FeedEventPartialFormat = "GameEvents/_{0}";
    public const string RankingEnabledViewData = "RankingEnabled";
    public const string EventPlaceholderPattern = @"\{(\d+)\}";
    public const string EventPlayerClass = "game-event-player";
    public const string EventHighlightClass = "game-event-highlight";
    public const string LinkTargetBlank = "_blank";
    public const string LinkRelExternal = "noopener noreferrer";
    public const char EventStar = '★';
    public const string ShortDateTimeFormat = "g";
}
