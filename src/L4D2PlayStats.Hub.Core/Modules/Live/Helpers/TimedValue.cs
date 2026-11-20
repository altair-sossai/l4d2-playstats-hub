using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Helpers;

public class TimedValue<T>(IClock clock, Func<TimeSpan> delay, TimeSpan expireIn, T initialValue)
{
    private readonly Lock _lock = new();
    private readonly Queue<(DateTime PublishAt, T Value)> _pending = new();
    private DateTime? _publishedAt;
    private T _value = default!;

    public TimedValue(IClock clock, TimeSpan delay, TimeSpan expireIn, T initialValue) : this(clock, () => delay, expireIn, initialValue)
    {
    }

    public T Value
    {
        get
        {
            lock (_lock)
            {
                Publish();

                return _publishedAt == null || clock.UtcNow >= _publishedAt.Value + expireIn ? initialValue : _value;
            }
        }
    }

    public void Set(T value)
    {
        lock (_lock)
        {
            _pending.Enqueue((clock.UtcNow + delay(), value));

            Publish();
        }
    }

    private void Publish()
    {
        var now = clock.UtcNow;

        while (_pending.Count > 0 && _pending.Peek().PublishAt <= now)
        {
            var pending = _pending.Dequeue();

            _value = pending.Value;
            _publishedAt = pending.PublishAt;
        }
    }
}
