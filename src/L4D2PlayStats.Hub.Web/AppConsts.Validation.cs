namespace L4D2PlayStats.Hub.Web;

public static partial class AppConsts
{
    public const string ActiveMenu = "active";

    public const string SavedTempData = "Saved";
    public const string ErrorTempData = "Error";
    public const string TokenTempData = "Token";
    public const string LiveSavedTempData = "LiveSaved";
    public const string LimitsSavedTempData = "LimitsSaved";
    public const string ReprocessedTempData = "Reprocessed";
    public const string RemovedTempData = "Removed";
    public const string BlockedTempData = "Blocked";
    public const string AlreadyBlockedTempData = "AlreadyBlocked";
    public const string UnblockedTempData = "Unblocked";
    public const string VoteRemovedTempData = "VoteRemoved";
    public const string RebuiltTempData = "Rebuilt";
    public const string ResetTempData = "Reset";
    public const string ForcedNameRemovedTempData = "ForcedNameRemoved";
    public const string ForcedNamesImportedTempData = "ForcedNamesImported";
    public const string QueuedTempData = "Queued";

    public const string InvalidSteamIdMessage = "That is not a valid SteamID64.";
    public const string SteamIdRequiredMessage = "The Steam ID must be provided.";
    public const string SteamIdInvalidMessage = "The Steam ID is invalid.";
    public const string NameRequiredMessage = "A name is required.";
    public const string CampaignRequiredMessage = "Write the name of the campaign and at least one map.";
    public const string ServerInUseMessage = "A server that already sent statistics cannot be deleted. Disable it instead.";
    public const string LastAdministratorMessage = "You cannot remove yourself or the last administrator.";
    public const string InvalidRemoteCommandMessage = "That is not a valid command. Use sm_command, !command or /command, without semicolons.";
    public const string AntiCheatModeInvalidMessage = "The mode is invalid.";
    public const string SkinPriceInvalidMessage = "The price is invalid.";
    public const string SkinRequiredMessage = "Choose the skin.";
    public const string PunishmentPointsInvalidMessage = "The experience points to remove must be greater than zero.";
}
