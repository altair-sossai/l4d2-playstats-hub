
namespace L4D2PlayStats.Hub.Core.Infrastructure;

public interface IClock
{
    DateTime UtcNow { get; }
}
