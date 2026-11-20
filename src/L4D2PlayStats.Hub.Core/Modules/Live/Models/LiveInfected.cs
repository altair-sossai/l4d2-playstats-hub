using L4D2PlayStats.Hub.Core.Modules.Live.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models;

public class LiveInfected : LivePlayer
{
    public InfectedType? Type { get; set; }
    public int Damage { get; set; }
    public int? Health { get; set; }
    public int? MaxHealth { get; set; }
    public bool IsInfectedGhost { get; set; }
    public bool IsPlayerAlive { get; set; }
}
