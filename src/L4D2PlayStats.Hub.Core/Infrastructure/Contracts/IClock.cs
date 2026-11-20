namespace L4D2PlayStats.Hub.Core.Infrastructure.Contracts;

public interface IClock
{
    DateTime UtcNow { get; }
}
