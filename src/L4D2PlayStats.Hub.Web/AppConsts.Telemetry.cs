using System.Text.RegularExpressions;

namespace L4D2PlayStats.Hub.Web;

public static partial class AppConsts
{
    public const string ApplicationInsightsConnectionStringKey = "APPLICATIONINSIGHTS_CONNECTION_STRING";
    public const float TelemetrySamplingRatio = 1.0f;
    public const LogLevel TelemetryMinimumLogLevel = LogLevel.Warning;
    public const string TelemetryLivePrefix = "/servers/";

    public static readonly string[] TelemetryIgnoredPaths = ["/api/server"];

    public static readonly Regex TelemetryComparisonPath = new(@"^/player/\d+/\d+/?$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static readonly string[] TelemetryLivePollingSuffixes = ["/header", "/players", "/feed"];

    public static readonly string[] TelemetryIgnoredPrefixes =
    [
        "/api/game-info",
        "/server/",
        "/api/external-chat",
        "/api/servers/heartbeat",
        "/api/skins",
        "/api/players/skins",
        "/avatar/",
        "/css/",
        "/js/",
        "/imgs/",
        "/lib/",
        "/fonts/",
        "/favicon",
        "/health"
    ];

    public static readonly string[] TelemetryIgnoredSegments = ["/game-info/"];
}
