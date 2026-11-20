using L4D2PlayStats.Hub.Core.Modules.Live.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Servers.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Helpers;

public class LiveChangeTracker
{
    private bool _started;
    private string? _header;
    private string? _players;
    private long _feed;
    private DateTime _headerNotifiedAt = DateTime.MinValue;
    private DateTime _playersNotifiedAt = DateTime.MinValue;
    private DateTime _feedNotifiedAt = DateTime.MinValue;

    public LiveChange Detect(LiveServerState state, GameServerSignal? signal, DateTime now, TimeSpan interval, TimeSpan feedInterval)
    {
        var snapshot = state.Snapshot();
        var header = LiveFingerprint.Header(snapshot, signal);
        var players = LiveFingerprint.Players(snapshot);
        var feed = state.FeedSequence;

        if (!_started)
        {
            _started = true;
            _header = header;
            _players = players;
            _feed = feed;

            return LiveChange.None;
        }

        var change = LiveChange.None;

        if (header != _header && now - _headerNotifiedAt >= interval)
        {
            _header = header;
            _headerNotifiedAt = now;
            change |= LiveChange.Header;
        }

        if (players != _players && now - _playersNotifiedAt >= interval)
        {
            _players = players;
            _playersNotifiedAt = now;
            change |= LiveChange.Players;
        }

        if (feed != _feed && now - _feedNotifiedAt >= feedInterval)
        {
            _feed = feed;
            _feedNotifiedAt = now;
            change |= LiveChange.Feed;
        }

        return change;
    }
}
