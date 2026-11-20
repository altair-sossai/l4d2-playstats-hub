using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;


namespace L4D2PlayStats.Hub.Core.Infrastructure;

public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
