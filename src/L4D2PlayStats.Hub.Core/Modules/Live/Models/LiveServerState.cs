using System.Collections.Concurrent;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;
using L4D2PlayStats.Hub.Core.Modules.Live.Settings;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models;

public class LiveServerState(IClock clock, Func<LiveServerSettings> settings)
{
    private static readonly TimeSpan PlayersExpiration = TimeSpan.FromMinutes(AppConsts.LivePlayersExpirationInMinutes);
    private static readonly TimeSpan SiteChatRetention = TimeSpan.FromMinutes(AppConsts.SiteChatRetentionInMinutes);

    private readonly Lock _chatLock = new();
    private readonly ConcurrentQueue<QueuedCommand> _commands = new();
    private readonly TimedValue<LiveConfiguration?> _configuration = new(clock, () => TimeSpan.FromSeconds(settings().DelayInSeconds), TimeSpan.FromMinutes(AppConsts.LiveConfigurationExpirationInMinutes), null);
    private readonly TimedFeed _feed = new(clock, () => TimeSpan.FromSeconds(settings().DelayInSeconds), TimeSpan.FromMinutes(AppConsts.LiveFeedRetentionInMinutes), AppConsts.LiveFeedMaxItems);
    private readonly TimedValue<IReadOnlyList<LiveInfected>> _infecteds = new(clock, () => TimeSpan.FromSeconds(settings().DelayInSeconds), PlayersExpiration, []);
    private readonly List<QueuedCommand> _internalCommands = [];
    private readonly Lock _internalCommandsLock = new();
    private readonly ConcurrentDictionary<string, string> _lastMessage = new();
    private readonly TimedValue<LiveRound?> _round = new(clock, () => TimeSpan.FromSeconds(settings().DelayInSeconds), TimeSpan.FromMinutes(AppConsts.LiveRoundExpirationInMinutes), null);
    private readonly TimedValue<LiveScoreboard?> _scoreboard = new(clock, () => TimeSpan.FromSeconds(settings().DelayInSeconds), TimeSpan.FromMinutes(AppConsts.LiveScoreboardExpirationInMinutes), null);
    private readonly List<SiteChatMessage> _siteMessages = [];
    private readonly TimedValue<IReadOnlyList<LivePlayer>> _spectators = new(clock, () => TimeSpan.FromSeconds(settings().DelayInSeconds), PlayersExpiration, []);
    private readonly TimedValue<IReadOnlyList<LiveSurvivor>> _survivors = new(clock, () => TimeSpan.FromSeconds(settings().DelayInSeconds), PlayersExpiration, []);

    public LiveServerState(IClock clock) : this(clock, () => new LiveServerSettings())
    {
    }

    public IReadOnlyList<FeedItem> Feed => _feed.Items;

    public long FeedSequence => _feed.Sequence;

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
        _survivors.Set([.. (command.Survivors ?? []).Where(survivor => survivor != null).OrderBy(survivor => survivor.Character)]);
        _infecteds.Set([.. (command.Infecteds ?? []).Where(infected => infected != null).OrderBy(infected => infected.Damage)]);
        _spectators.Set([.. (command.Spectators ?? []).Where(spectator => spectator != null).OrderBy(spectator => spectator.Name, StringComparer.CurrentCulture)]);
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

    public SiteChatResult AddSiteMessage(SiteChatUser user, string? text, bool skipCooldown, SiteChatSettings? chatSettings = null)
    {
        var limits = chatSettings ?? new SiteChatSettings();
        var message = text?.Trim();

        if (string.IsNullOrEmpty(message))
            return SiteChatResult.Failed(SiteChatError.EmptyMessage);

        if (message.Length > limits.MaxMessageLength)
            return SiteChatResult.Failed(SiteChatError.MessageTooLong);

        lock (_chatLock)
        {
            var now = clock.UtcNow;

            RemoveOldSiteMessages();

            if (!skipCooldown && OnCooldown(user.CommunityId, now, limits))
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
            if (clock.UtcNow - command.QueuedAtUtc < TimeSpan.FromMinutes(settings().RemoteCommandExpirationInMinutes))
                return command;

        return null;
    }

    public bool HasInternal(string fullCommand)
    {
        lock (_internalCommandsLock)
        {
            RemoveExpiredInternalCommands();

            return _internalCommands.Any(command => command.Command.FullCommand == fullCommand);
        }
    }

    public void EnqueueInternal(QueuedCommand command)
    {
        lock (_internalCommandsLock)
        {
            _internalCommands.Add(command);
        }
    }

    public QueuedCommand? DequeueInternal()
    {
        lock (_internalCommandsLock)
        {
            RemoveExpiredInternalCommands();

            if (_internalCommands.Count == 0)
                return null;

            var command = _internalCommands[0];
            _internalCommands.RemoveAt(0);

            return command;
        }
    }

    private void RemoveExpiredInternalCommands()
    {
        var now = clock.UtcNow;

        _internalCommands.RemoveAll(command => now - command.QueuedAtUtc >= TimeSpan.FromMinutes(settings().RemoteCommandExpirationInMinutes));
    }

    private bool OnCooldown(long communityId, DateTime now, SiteChatSettings limits)
    {
        var last = _siteMessages.LastOrDefault();
        if (last != null && now - last.When < TimeSpan.FromSeconds(limits.GlobalCooldownInSeconds))
            return true;

        var lastFromUser = _siteMessages.LastOrDefault(message => message.UserId == communityId);

        return lastFromUser != null && now - lastFromUser.When < TimeSpan.FromSeconds(limits.UserCooldownInSeconds);
    }

    private void RemoveOldSiteMessages()
    {
        var now = clock.UtcNow;

        _siteMessages.RemoveAll(message => now - message.When >= SiteChatRetention);
    }
}