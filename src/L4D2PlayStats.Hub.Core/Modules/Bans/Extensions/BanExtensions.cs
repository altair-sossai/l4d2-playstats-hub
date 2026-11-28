using L4D2PlayStats.Hub.Core.Modules.Bans.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Bans.Extensions;

public static class BanExtensions
{
    public static BanStatus Status(this Ban ban, DateTime now)
    {
        if (ban.LiftedAtUtc != null)
            return BanStatus.Lifted;

        return ban.EndsAtUtc == null || ban.EndsAtUtc > now ? BanStatus.Active : BanStatus.Expired;
    }

    public static int MinutesLeft(this Ban ban, DateTime now)
    {
        if (ban.EndsAtUtc == null)
            return 0;

        return Math.Max(1, (int)Math.Ceiling((ban.EndsAtUtc.Value - now).TotalMinutes));
    }
}
