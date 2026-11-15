using L4D2PlayStats.Hub.Core.Modules.Settings.Models;

namespace L4D2PlayStats.Hub.Web.Models;

public class SetupViewModel
{
    public string Step { get; init; } = null!;
    public string? Token { get; init; }
    public string? Error { get; init; }
    public bool DevelopmentSignIn { get; init; }
    public SettingsViewModel Settings { get; init; } = new();
    public ModuleSettings Modules { get; init; } = new();
    public ServerViewModel? Server { get; init; }
}
