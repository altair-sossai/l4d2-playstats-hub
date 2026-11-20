using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;

namespace L4D2PlayStats.Hub.Tests.Support;

public class FakeClock : IClock
{
    public DateTime UtcNow { get; set; } = new(2026, 11, 15, 12, 0, 0, DateTimeKind.Utc);

    public void Advance(TimeSpan time)
    {
        UtcNow = UtcNow.Add(time);
    }
}
