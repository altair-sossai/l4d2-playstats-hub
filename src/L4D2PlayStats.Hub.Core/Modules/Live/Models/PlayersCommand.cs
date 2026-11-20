namespace L4D2PlayStats.Hub.Core.Modules.Live.Models;

public class PlayersCommand
{
    public List<LiveSurvivor>? Survivors { get; set; }
    public List<LiveInfected>? Infecteds { get; set; }
    public List<LivePlayer>? Spectators { get; set; }
}