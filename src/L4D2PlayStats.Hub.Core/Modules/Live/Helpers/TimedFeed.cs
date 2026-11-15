using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Helpers;

public class TimedFeed(IClock clock, TimeSpan delay, TimeSpan expireIn, int maxItems)
{
    private readonly List<(DateTime PublishedAt, FeedItem Item)> _items = [];
    private readonly Lock _lock = new();
    private readonly Queue<(DateTime PublishAt, FeedItem Item)> _pending = new();
    private long _sequence;

    public IReadOnlyList<FeedItem> Items
    {
        get
        {
            lock (_lock)
            {
                Refresh();

                return [.. _items.Select(entry => entry.Item)];
            }
        }
    }

    public long Sequence
    {
        get
        {
            lock (_lock)
            {
                return _sequence;
            }
        }
    }

    public void Add(FeedItem item, bool delayed)
    {
        lock (_lock)
        {
            var now = clock.UtcNow;

            if (delayed && delay > TimeSpan.Zero)
                _pending.Enqueue((now + delay, item));
            else
                Insert(now, item);

            Refresh();
        }
    }

    private void Refresh()
    {
        var now = clock.UtcNow;

        while (_pending.Count > 0 && _pending.Peek().PublishAt <= now)
        {
            var pending = _pending.Dequeue();

            Insert(pending.PublishAt, pending.Item);
        }

        _items.RemoveAll(entry => now >= entry.PublishedAt + expireIn);
    }

    private void Insert(DateTime publishedAt, FeedItem item)
    {
        var index = _items.Count;

        while (index > 0 && _items[index - 1].Item.When > item.When)
            index--;

        item.Sequence = ++_sequence;
        _items.Insert(index, (publishedAt, item));

        while (_items.Count > maxItems)
            _items.RemoveAt(0);
    }
}
