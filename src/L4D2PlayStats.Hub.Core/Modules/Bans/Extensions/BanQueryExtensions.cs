namespace L4D2PlayStats.Hub.Core.Modules.Bans.Extensions;

public static class BanQueryExtensions
{
    public static IQueryable<Ban> Active(this IQueryable<Ban> query, DateTime now)
    {
        return query.Where(ban => ban.LiftedAtUtc == null && (ban.EndsAtUtc == null || ban.EndsAtUtc > now));
    }

    public static IQueryable<Ban> Ended(this IQueryable<Ban> query, DateTime now)
    {
        return query.Where(ban => ban.LiftedAtUtc != null || (ban.EndsAtUtc != null && ban.EndsAtUtc <= now));
    }
}
