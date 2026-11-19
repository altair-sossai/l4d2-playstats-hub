namespace L4D2PlayStats.Hub.Web;

public static partial class AppConsts
{
    public const string ApplicationInsightsConnectionStringKey = "APPLICATIONINSIGHTS_CONNECTION_STRING";
    public const float TelemetrySamplingRatio = 1.0f;

    public static readonly string[] TelemetryIgnoredPrefixes =
    [
        "/api/game-info",
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
