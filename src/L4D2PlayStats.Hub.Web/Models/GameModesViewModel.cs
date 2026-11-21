using L4D2PlayStats.Hub.Core.Modules.GameModes;

namespace L4D2PlayStats.Hub.Web.Models;

public class GameModesViewModel
{
    public PagedListModel<GameModeRule> Rules { get; init; } = null!;
    public PagedListModel<SeenConfiguration> Configurations { get; init; } = null!;
    public string ReturnUrl { get; init; } = AppConsts.AdminGameModesUrl;
}
