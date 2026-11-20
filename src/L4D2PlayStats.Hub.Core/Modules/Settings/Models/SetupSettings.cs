using L4D2PlayStats.Hub.Core.Modules.Settings.Attributes;

namespace L4D2PlayStats.Hub.Core.Modules.Settings.Models;

public class SetupSettings
{
    [Secret]
    public string? Token { get; set; }

    public bool Completed { get; set; }
}