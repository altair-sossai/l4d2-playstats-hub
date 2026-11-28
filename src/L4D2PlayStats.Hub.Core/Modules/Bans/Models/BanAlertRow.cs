namespace L4D2PlayStats.Hub.Core.Modules.Bans.Models;

public class BanAlertRow
{
    public required BanAlert Alert { get; init; }
    public required Ban Ban { get; init; }
}
