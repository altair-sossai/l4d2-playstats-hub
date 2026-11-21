namespace L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;

public interface ISiteClock
{
    Task<TimeZoneInfo> ZoneAsync(CancellationToken cancellationToken = default);
    Task<DateTime> ToLocalAsync(DateTime utc, CancellationToken cancellationToken = default);
}
