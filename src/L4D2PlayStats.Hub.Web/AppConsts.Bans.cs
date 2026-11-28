namespace L4D2PlayStats.Hub.Web;

public static partial class AppConsts
{
    public const string BansUrl = "/bans";
    public const string BansAdminUrl = "/admin/bans";
    public const string BansApiRoute = "api/bans";
    public const int BansPageSize = 20;
    public const int BansPublicPageSize = 25;
    public const string BansActiveTab = "active";
    public const string BansEndedTab = "ended";
    public const string BansAlertsTab = "alerts";
    public const string BansEndedPageQuery = "endedPage";
    public const string BansAlertsPageQuery = "alertsPage";
    public const string BanPermanentDuration = "0";
    public const string BanCustomDuration = "custom";
    public const string BanVoteCommand = "sm_voteban";
    public static readonly int[] BanDurationsInMinutes = [60, 1440, 4320, 10080, 20160, 43200, 129600, 525600];
    public const string BanSavedTempData = "BanSaved";
    public const string BanLiftedTempData = "BanLifted";
    public const string BanAlertIgnoredTempData = "BanAlertIgnored";
    public const string BansImportedTempData = "BansImported";
    public const string BanAlreadyActiveMessage = "This player is already banned. Edit the ban on the list instead.";
    public const string BanInvalidEndMessage = "Choose when the ban ends: a date in the future, or permanent.";
    public const string BanNotFoundMessage = "This ban is no longer active.";
    public const string BanAlertNotFoundMessage = "This alert was already handled.";
    public const string BanBadRequestMessage = "A valid player is required";
    public const string BanDateFormat = "yyyy-MM-dd HH:mm";
}
