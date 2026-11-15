using L4D2PlayStats.Hub.Core.Modules.Live.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models;

public class LiveRound
{
    public bool IsInReady { get; set; }
    public bool IsInPause { get; set; }
    public bool InSecondHalfOfRound { get; set; }
    public int MaxChapterProgressPoints { get; set; }
    public decimal TankPercent { get; set; }
    public decimal WitchPercent { get; set; }
    public AntiCheatStatus? AntiCheatStatus { get; set; }
    public bool TankDisabled => TankPercent <= 0;
    public bool WitchDisabled => WitchPercent <= 0;
}
