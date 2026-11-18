namespace L4D2PlayStats.Hub.Web;

public static partial class AppConsts
{
    public const string TrustedProxiesSetting = "Hub:TrustedProxies";
    public static readonly char[] TrustedProxySeparators = [',', ';', ' ', '\n', '\r', '\t'];

    public const string ErrorPath = "/error";
    public const string ErrorStatusPath = "/error/{0}";
    public const int ErrorStatusCodeLimit = 600;
    public const string InGamePath = "/ingame";
    public const string HtmlContentType = "text/html";
    public const string PlainTextContentType = "text/plain";
    public static readonly string[] ErrorPageExcludedPaths = ["/api", InGamePath];
}
