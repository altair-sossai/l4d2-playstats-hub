using System.Collections.Concurrent;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Live.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models;

public class LiveServerState(IClock clock)
{
    private static readonly TimeSpan Delay = TimeSpan.FromSeconds(AppConsts.LiveDelayInSeconds);
    private static readonly TimeSpan PlayersExpiration = TimeSpan.FromMinutes(AppConsts.LivePlayersExpirationInMinutes);
    private static readonly TimeSpan SiteChatRetention = TimeSpan.FromMinutes(AppConsts.SiteChatRetentionInMinutes);
    private static readonly TimeSpan GlobalCooldown = TimeSpan.FromSeconds(AppConsts.SiteChatGlobalCooldownInSeconds);
    private static readonly TimeSpan UserCooldown = TimeSpan.FromSeconds(AppConsts.SiteChatUserCooldownInSeconds);
    private static readonly TimeSpan CommandExpiration = TimeSpan.FromMinutes(AppConsts.RemoteCommandExpirationInMinutes);

    private readonly Lock _chatLock = new();
    private readonly ConcurrentQueue<QueuedCommand> _commands = new();
    private readonly TimedValue<LiveConfiguration?> _configuration = new(clock, Delay, TimeSpan.FromMinutes(AppConsts.LiveConfigurationExpirationInMinutes), null);
    private readonly TimedFeed _feed = new(clock, Delay, TimeSpan.FromMinutes(AppConsts.LiveFeedRetentionInMinutes), AppConsts.LiveFeedMaxItems);
    private readonly TimedValue<IReadOnlyList<LiveInfected>> _infecteds = new(clock, Delay, PlayersExpiration, []);
    private readonly ConcurrentDictionary<string, string> _lastMessage = new();
    private readonly TimedValue<LiveRound?> _round = new(clock, Delay, TimeSpan.FromMinutes(AppConsts.LiveRoundExpirationInMinutes), null);
    private readonly TimedValue<LiveScoreboard?> _scoreboard = new(clock, Delay, TimeSpan.FromMinutes(AppConsts.LiveScoreboardExpirationInMinutes), null);
    private readonly List<SiteChatMessage> _siteMessages = [];
    private readonly TimedValue<IReadOnlyList<LivePlayer>> _spectators = new(clock, Delay, PlayersExpiration, []);
    private readonly TimedValue<IReadOnlyList<LiveSurvivor>> _survivors = new(clock, Delay, PlayersExpiration, []);

    public IReadOnlyList<FeedItem> Feed => _feed.Items;

    public IReadOnlyList<SiteChatMessage> SiteMessages
    {
        get
        {
            lock (_chatLock)
            {
                RemoveOldSiteMessages();

                return [.. _siteMessages];
            }
        }
    }

    public int PendingCommands => _commands.Count;

    public LiveSnapshot Snapshot()
    {
        return new LiveSnapshot
        {
            Configuration = _configuration.Value,
            Round = _round.Value,
            Scoreboard = _scoreboard.Value,
            Survivors = _survivors.Value,
            Infecteds = _infecteds.Value,
            Spectators = _spectators.Value,
            Feed = _feed.Items
        };
    }

    public IReadOnlyList<FeedItem> FeedAfter(long sequence)
    {
        var items = _feed.Items;
        var from = sequence > _feed.Sequence ? 0 : sequence;

        return [.. items.Where(item => item.Sequence > from)];
    }

    public IReadOnlyList<SiteChatMessage> SiteMessagesAfter(long ticks)
    {
        return [.. SiteMessages.Where(message => message.When.Ticks > ticks)];
    }

    public void Update(LiveConfiguration? configuration)
    {
        _configuration.Set(configuration);
    }

    public void Update(LiveRound? round)
    {
        _round.Set(round);
    }

    public void Update(LiveScoreboard? scoreboard)
    {
        _scoreboard.Set(scoreboard);
    }

    public void Update(PlayersCommand command)
    {
        _survivors.Set([.. (command.Survivors ?? []).OfType<LiveSurvivor>().OrderBy(survivor => survivor.Character)]);
        _infecteds.Set([.. (command.Infecteds ?? []).OfType<LiveInfected>().OrderBy(infected => infected.Damage)]);
        _spectators.Set([.. (command.Spectators ?? []).OfType<LivePlayer>().OrderBy(spectator => spectator.Name, StringComparer.CurrentCulture)]);
    }

    public bool AddMessage(ChatMessageCommand command)
    {
        var message = command.Message?.Trim();

        if (string.IsNullOrEmpty(command.CommunityId) || string.IsNullOrEmpty(message))
            return false;

        if (ChatBindBlacklist.IsBind(message))
            return false;

        if (_lastMessage.TryGetValue(command.CommunityId, out var last) && last.Equals(message, StringComparison.CurrentCultureIgnoreCase))
            return false;

        _lastMessage[command.CommunityId] = message;
        _feed.Add(new LiveChatMessage(command, clock.UtcNow), true);

        return true;
    }

    public void AddEvent(GameEvent gameEvent)
    {
        gameEvent.When = clock.UtcNow;
        _feed.Add(gameEvent, true);
    }

    public SiteChatResult AddSiteMessage(SiteChatUser user, string? text, bool skipCooldown)
    {
        var message = text?.Trim();

        if (string.IsNullOrEmpty(message))
            return SiteChatResult.Failed(SiteChatError.EmptyMessage);

        if (message.Length > AppConsts.SiteChatMaxMessageLength)
            return SiteChatResult.Failed(SiteChatError.MessageTooLong);

        lock (_chatLock)
        {
            var now = clock.UtcNow;

            RemoveOldSiteMessages();

            if (!skipCooldown && OnCooldown(user.CommunityId, now))
                return SiteChatResult.Failed(SiteChatError.RateLimited);

            var siteMessage = new SiteChatMessage(user, message, now);

            _siteMessages.Add(siteMessage);
            _feed.Add(new LiveChatMessage(siteMessage), false);
        }

        return SiteChatResult.Sent();
    }

    public void Enqueue(QueuedCommand command)
    {
        _commands.Enqueue(command);
    }

    public QueuedCommand? Dequeue()
    {
        while (_commands.TryDequeue(out var command))
            if (clock.UtcNow - command.QueuedAtUtc < CommandExpiration)
                return command;

        return null;
    }

    private bool OnCooldown(long communityId, DateTime now)
    {
        var last = _siteMessages.LastOrDefault();
        if (last != null && now - last.When < GlobalCooldown)
            return true;

        var lastFromUser = _siteMessages.LastOrDefault(message => message.UserId == communityId);

        return lastFromUser != null && now - lastFromUser.When < UserCooldown;
    }

    private void RemoveOldSiteMessages()
    {
        var now = clock.UtcNow;

        _siteMessages.RemoveAll(message => now - message.When >= SiteChatRetention);
    }
}
