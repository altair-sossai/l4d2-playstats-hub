namespace L4D2PlayStats.Hub.Web;

public static partial class AppConsts
{
    public const int LogoBytesPerMegabyte = 1048576;
    public const string LogoUnsupportedMessage = "The logo was not changed: it must be a PNG, JPG, WEBP or GIF image.";
    public const string LogoTooLargeMessage = "The logo was not changed: the file is too large.";

    public const string GoogleAnalyticsIdPattern = "^[A-Za-z0-9-]{1,40}$";
    public const string GoogleAnalyticsIdInvalidMessage = "The Google Analytics ID was not changed: it can only have letters, digits and hyphens.";
    public const string GoogleAnalyticsIdPlaceholder = "G-XXXXXXXXXX";

    public const string FooterLinksIgnoredMessage = "Some footer links were left out: each line must be a text, a vertical bar and an address that starts with http:// or https://.";
    public const string FooterLinkExample = "Discord | https://discord.gg/example";

    public const int MetaKeywordsMaxLength = 500;

    public const string SettingsProblemsTempData = "SettingsProblems";

    public const string SetupWizardUrl = "/setup";
    public const string ThemeSwitchPath = "/theme";
    public const string AdminNavAntiCheatUrl = "/admin/anti-cheat";
    public const string AdminSteamIdPlaceholder = "7656119...";
    public const string AdminServerAddressPlaceholder = "play.example.com:27015";
    public const string AdminPublicUrlPlaceholder = "https://stats.example.com";
}
