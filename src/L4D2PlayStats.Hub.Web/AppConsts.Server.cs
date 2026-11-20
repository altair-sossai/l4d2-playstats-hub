namespace L4D2PlayStats.Hub.Web;

public static partial class AppConsts
{
    public const int LivePageRequestTimeoutInMilliseconds = 2000;
    public const int LivePageMaxFeedItems = 300;
    public const int ServerPickerRefreshInMilliseconds = 10000;
    public const string LiveHubUrl = "/hubs/live";
    public const string LiveHubGroupPrefix = "live:";
    public const string LiveHubChangedMethod = "changed";
    public const string LiveChangeHeader = "header";
    public const string LiveChangePlayers = "players";
    public const string LiveChangeFeed = "feed";
    public const int LivePushIntervalInMilliseconds = 500;
    public const int LivePageSafetyRefreshInMilliseconds = 30000;
    public const int LiveHubRetryInMilliseconds = 30000;
    public const int LiveHubLongPollingTimeoutInSeconds = 50;
    public const string ProxyBufferingHeader = "X-Accel-Buffering";
    public const string ProxyBufferingDisabled = "no";
    public const int ChatBansPageSize = 20;
    public const string ServersUrl = "/servers";
    public const string ServersApiUrl = "/api/servers";
    public const string ChatBansUrl = "/admin/chat-bans";
    public const string PlayerUrlPrefix = $"{PlayerUrl}/";
    public const string AvatarUrl = "/avatar/";
    public const string CopyIconLight = "copy-light.png";
    public const string CopyIconDark = "copy-dark.png";
    public const string IconThemeLight = "light";
    public const string IconThemeDark = "dark";
    public const string FeedKindMessage = "message";
    public const string FeedKindEvent = "event";
    public const string FeedMessagePartial = "_Message";
    public const string FeedEventPartialFormat = "GameEvents/_{0}";
    public const string EventPlaceholderPattern = @"\{(\d+)\}";
    public const string EventPlayerClass = "game-event-player";
    public const string EventHighlightClass = "game-event-highlight";
    public const string LinkTargetBlank = "_blank";
    public const char EventStar = '★';
}
