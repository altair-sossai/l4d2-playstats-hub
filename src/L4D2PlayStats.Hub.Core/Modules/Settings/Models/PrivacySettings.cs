using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Settings.Models;

public class PrivacySettings
{
    public RelatedAccountsVisibility RelatedAccounts { get; set; } = RelatedAccountsVisibility.Everyone;
    public int ConnectionRetentionInMonths { get; set; } = AppConsts.PlayerConnectionRetentionInMonths;
}