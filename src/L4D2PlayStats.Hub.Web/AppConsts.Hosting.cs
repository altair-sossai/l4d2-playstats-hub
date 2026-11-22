namespace L4D2PlayStats.Hub.Web;

public static partial class AppConsts
{
    public const string TrustedProxiesSetting = "Hub:TrustedProxies";

    public const string ErrorPath = "/error";
    public const string ErrorStatusPath = "/error/{0}";
    public const int ErrorStatusCodeLimit = 600;
    public const string ExceptionLoggedItem = "hub-exception-logged";
    public const string InGamePath = "/ingame";
    public const string InGamePlayerPath = "/ingame/player";
    public const string InGameMatchesPath = "/ingame/matches";
    public const string InGameCommandsPath = "/ingame/commands";
    public const string InGameQueryKey = "steamid";
    public const string InGameCookie = "hub-in-game";
    public const string InGameCookieValue = "1";
    public const string RobotUserAgentPattern = "bot|spider|crawl|slurp";
    public const string HtmlContentType = "text/html";
    public const string PlainTextContentType = "text/plain";

    public const string HealthCommand = "health";
    public const string HealthPath = "/api/ping";
    public const string HealthHost = "127.0.0.1";
    public const int HealthTimeoutInSeconds = 5;
    public const string DefaultHttpPort = "5000";
    public const string UrlsVariable = "ASPNETCORE_URLS";
    public const string HttpPortsVariable = "ASPNETCORE_HTTP_PORTS";
    public const string ListenUrlPattern = @"^(?<scheme>https?)://(?<host>\[[^\]]*\]|[^:/]+)(:(?<port>\d+))?";
    public static readonly char[] TrustedProxySeparators = [',', ';', ' ', '\n', '\r', '\t'];
    public static readonly string[] ErrorPageExcludedPaths = ["/api", InGamePath, LiveHubUrl];
    public static readonly string[] WildcardHosts = ["+", "*", "0.0.0.0", "[::]"];
}
