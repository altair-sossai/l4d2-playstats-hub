namespace L4D2PlayStats.Hub.Core;

public static partial class AppConsts
{
    public const int BanReasonMaxLength = 300;
    public const int BanCommandMaxLength = 64;
    public const int BanLiftReasonMaxLength = 300;
    public const string BanSyncCommand = "sm_playstats_ban_sync";
    public const string BanCommentPrefix = "//";
    public const string BanFileNameField = "name";
    public const string BanFileDateField = "ban date";
    public const string BanFileExpireField = "expire in";
    public const string BanFileReasonField = "ban reason";
    public const string BanFileNever = "never";
    public const string BanFileLinePattern = @"^\s*banid\s+(\d+)\s+(\S+)";
    public const string BanFileDateFormat = "yyyy-MM-dd";
    public const string BansPath = "/bans";
    public const string BansAlertsPath = "/admin/bans?tab=alerts";
}
