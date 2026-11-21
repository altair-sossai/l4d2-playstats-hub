namespace L4D2PlayStats.Hub.Web;

public static partial class AppConsts
{
    public const string LogoUnsupportedMessage = "The logo was not changed: it must be a PNG, JPG, WEBP or GIF image.";
    public const string LogoTooLargeMessage = "The logo was not changed: the file is too large.";

    public const string GoogleAnalyticsIdPattern = "^[A-Za-z0-9-]{1,40}$";
    public const string GoogleAnalyticsIdInvalidMessage = "The Google Analytics ID was not changed: it can only have letters, digits and hyphens.";
    public const string GoogleAnalyticsIdPlaceholder = "G-XXXXXXXXXX";

    public const string FooterLinksIgnoredMessage = "Some footer links were left out: each line must be a text, a vertical bar and an address that starts with http:// or https://.";
    public const string FooterLinkExample = "Discord | https://discord.gg/example";
    public const string AdminTabQuery = "tab";
    public const string AdminTabPanePrefix = "tab-";
    public const string AdminTabsScriptItem = "admin-tabs-script";
    public const string SettingsSiteTab = "site";
    public const string SettingsAppearanceTab = "appearance";
    public const string SettingsSeasonsTab = "seasons";
    public const string SettingsRankingTab = "ranking";
    public const string SettingsPrivacyTab = "privacy";
    public const string BackupStatusTab = "status";
    public const string BackupStorageTab = "storage";
    public const string AntiCheatPlayersTab = "players";
    public const string AntiCheatPageTab = "page";
    public const string ChatBansListTab = "bans";
    public const string ChatBansLimitsTab = "limits";
    public const string ServersListTab = "servers";
    public const string ServersLiveTab = "live";
    public const string GameModesRulesTab = "rules";
    public const string GameModesSeenTab = "seen";
    public const string FooterLinkTextExample = "Discord";
    public const string FooterLinkUrlExample = "https://discord.gg/example";
    public const int FooterLinkTextMaxLength = 100;

    public const int MetaKeywordsMaxLength = 500;

    public const string SettingsProblemsTempData = "SettingsProblems";

    public const string SetupWizardUrl = "/setup";
    public const string ThemeSwitchPath = "/theme";
    public const string AdminSteamIdPlaceholder = "7656119...";
    public const string AdminServerAddressPlaceholder = "play.example.com:27015";
    public const string AdminPublicUrlPlaceholder = "https://stats.example.com";
    public const string RelatedAccountsChatCvar = "gameinfo_related_accounts_chat";

    public const string AdminUrl = "/admin";
    public const string AdminServersUrl = "/admin/servers";
    public const string AdminModulesUrl = "/admin/modules";
    public const string AdminSettingsUrl = "/admin/settings";
    public const string AdminGameModesUrl = "/admin/game-modes";
    public const string AdminCampaignsUrl = "/admin/campaigns";
    public const string AdminPatentsUrl = "/admin/patents";
    public const string AdminAdministratorsUrl = "/admin/administrators";
    public const string AdminCommunityUrl = "/admin/community";
    public const string RemoteCommandsUrl = "/admin/remote-commands";
    public const string PlayerConnectionsUrl = "/admin/player-connections";
    public const string ForcedNamesUrl = "/admin/forced-names";
    public const string ForcedNamesApiRoute = "api/forced-names";
    public const int ForcedNamesPageSize = 20;
    public const string AdminPageQuery = "page";
    public const string GameModesSeenPageQuery = "seenPage";
    public const int AdministratorsPageSize = 20;
    public const int GameServersPageSize = 20;
    public const int CampaignsPageSize = 20;
    public const int GameModeRulesPageSize = 20;
    public const int GameConfigurationsPageSize = 20;
    public const int GameModeNewRulePriority = 50;
    public const string ForcedNameInvalidPlayerMessage = "That is not a valid SteamID64.";
    public const string ForcedNameInvalidNameMessage = "Write the name the player must use.";
    public const string AdminMenuId = "admin-menu-items";
}
