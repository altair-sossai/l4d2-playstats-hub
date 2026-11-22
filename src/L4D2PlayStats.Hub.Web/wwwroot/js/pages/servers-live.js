var serversLiveConfig = JSON.parse(document.getElementById("servers-live-config").textContent);

var serverUrl = serversLiveConfig.values.serversUrl + '/' + serversLiveConfig.values.publicId;
var serverPublicId = serversLiveConfig.values.publicId;
var refreshInterval = serversLiveConfig.values.refreshInterval;
var safetyRefreshInterval = serversLiveConfig.values.livePageSafetyRefreshInMilliseconds;
var liveHubUrl = serversLiveConfig.values.liveHubUrl;
var liveHubRetry = serversLiveConfig.values.liveHubRetryInMilliseconds;
var requestTimeout = serversLiveConfig.values.livePageRequestTimeoutInMilliseconds;
var maxFeedItems = serversLiveConfig.values.livePageMaxFeedItems;
var feedAfter = 0;
var pushConnected = false;
var dd = new diffDOM.DiffDOM();

function sync(current, html) {
    var source = document.createElement('div');
    source.id = current.id;
    source.innerHTML = html;

    var diffs = dd.diff(current, source);
    dd.apply(current, diffs);
}

function createRefresher(load, interval) {
    var refresher = { timer: null, busy: false, pending: false };

    refresher.run = function () {
        clearTimeout(refresher.timer);
        refresher.timer = null;

        if (refresher.busy) {
            refresher.pending = true;
            return;
        }

        refresher.busy = true;
        refresher.pending = false;

        load(function () {
            refresher.busy = false;
            refresher.timer = setTimeout(refresher.run, refresher.pending ? 0 : pushConnected ? safetyRefreshInterval : interval);
        });
    };

    return refresher;
}

function loadHeader(done) {
    $.ajax({
        url: serverUrl + '/header',
        type: 'GET',
        timeout: requestTimeout,
        success: function (html) {
            var target = document.getElementById('game-info-header');
            sync(target, html);

        },
        complete: done
    });
}

function loadPlayers(done) {
    $.ajax({
        url: serverUrl + '/players',
        type: 'GET',
        timeout: requestTimeout,
        success: function (html) {
            var target = document.getElementById('game-info-players');
            sync(target, html);

            $('.game-info-chat-own').toggle(!$(target).children('div').first().hasClass('hidden'));
        },
        complete: done
    });
}

function applyFeedFilter() {
    var mode = 'all';

    try { 
        mode = localStorage.getItem('feedFilter') || 'all'; 
    } catch (err) {
        console.warn('Unable to access localStorage:', err);
    }

    $('#feed p[data-feed-kind]').each(function () {
        var kind = $(this).attr('data-feed-kind');
        var show = mode === 'all'
            || (mode === 'events' && kind === 'event')
            || (mode === 'messages' && kind === 'message');
        $(this).toggle(show);
    });
}

function lastFeedSequence(items) {
    var last = 0;

    items.each(function () {
        last = Math.max(last, Number($(this).attr('data-feed-sequence')) || 0);
    });

    return last;
}

function loadFeed(done) {
    var after = feedAfter;

    $.ajax({
        url: serverUrl + '/feed?after=' + after,
        type: 'GET',
        timeout: requestTimeout,
        success: function (html, status) {
            if (!html || status == "nocontent")
                return;

            var feed = $('#feed');
            var scrollDown = feed[0].scrollHeight - feed.scrollTop() === feed.outerHeight();

            feedAfter = lastFeedSequence($('<div>').html(html).find('p[data-feed-sequence]')) || feedAfter;

            feed.append(html);

            $('#feed p[data-feed-when]').each(function () {
                var feedWhen = $(this).data('feed-when');
                if (feedWhen) {
                    var $duplicates = $('#feed p[data-feed-when="' + feedWhen + '"]').slice(1);
                    $duplicates.remove();
                }
            });

            var $items = $('#feed p[data-feed-when]');
            if ($items.length > maxFeedItems)
                $items.slice(0, $items.length - maxFeedItems).remove();

            applyFeedFilter();

            if (scrollDown)
                feed.scrollTop(feed[0].scrollHeight);
        },
        complete: done
    });
}

var refreshers = {};

refreshers[serversLiveConfig.values.liveChangeHeader] = createRefresher(loadHeader, refreshInterval);
refreshers[serversLiveConfig.values.liveChangePlayers] = createRefresher(loadPlayers, refreshInterval);
refreshers[serversLiveConfig.values.liveChangeFeed] = createRefresher(loadFeed, refreshInterval / 2);

function refreshAll() {
    $.each(refreshers, function (name, refresher) {
        refresher.run();
    });
}

function refreshFeed() {
    refreshers[serversLiveConfig.values.liveChangeFeed].run();
}

function setPushConnected(connected) {
    if (pushConnected === connected)
        return;

    pushConnected = connected;
    refreshAll();
}

function startPush() {
    if (typeof signalR === 'undefined' || !window.Promise)
        return;

    var connection;

    try {
        connection = new signalR.HubConnectionBuilder()
            .withUrl(liveHubUrl)
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.None)
            .build();
    } catch (err) {
        return;
    }

    function watch() {
        return connection.invoke(serversLiveConfig.values.watchMethod, serverPublicId)
            .then(function (watching) {
                setPushConnected(watching === true);
            })
            .catch(function () {
                setPushConnected(false);
            });
    }

    var retryTimer = null;

    function retry() {
        if (retryTimer)
            return;

        retryTimer = setTimeout(function () {
            retryTimer = null;
            connect();
        }, liveHubRetry);
    }

    function connect() {
        if (connection.state !== signalR.HubConnectionState.Disconnected)
            return;

        connection.start().then(watch, retry);
    }

    connection.on(serversLiveConfig.values.liveHubChangedMethod, function (changes) {
        $.each(changes || [], function (index, change) {
            var refresher = refreshers[change];

            if (refresher)
                refresher.run();
        });
    });

    connection.onreconnecting(function () {
        setPushConnected(false);
    });

    connection.onreconnected(watch);

    connection.onclose(function () {
        setPushConnected(false);
        retry();
    });

    connect();
}

function sendMessage() {
    var message = $('#message').val().trim();
    if (!message)
        return;

    $.ajax({
        url: serverUrl + '/chat',
        type: 'POST',
        data: { message: message, __RequestVerificationToken: $('#chatForm input[name="__RequestVerificationToken"]').val() },
        success: function (result, statusCode) {
            if (result?.success || statusCode == 'nocontent') {
                refreshFeed();
                $('#message').val('');
                $('button[type="submit"]').prop('disabled', true);
            } else {
                alert(result?.errorMessage ?? statusCode);
            }
        },
        error: function (jqXHR) {
            if (jqXHR.status === 403) {
                alert(jqXHR.responseJSON?.errorMessage ?? serversLiveConfig.texts["You have been banned from the chat."]);
                location.reload();
            } else {
                alert(jqXHR.responseJSON?.errorMessage ?? jqXHR.statusText);
            }
        }
    });
}

terminal.apply = function (command) {
    $('#message').val(command);
    $('button[type="submit"]').prop('disabled', false);

    if (!confirm(serversLiveConfig.texts["Are you sure you want to execute the following command?"] + '\n\n' + command))
        return false;

    $("#chatForm").submit();

    return true;
};

$(document).ready(function () {
    feedAfter = lastFeedSequence($('#feed p[data-feed-sequence]'));

    refreshAll();
    startPush();

    var savedFilter = 'all';

    try { 
        savedFilter = localStorage.getItem('feedFilter') || 'all'; 
    } 
    catch (err) {
        console.warn('Unable to access localStorage:', err);
    }

    $('#feed-filter-label').text($('#feed-filter-menu a[data-filter="' + savedFilter + '"]').text());

    applyFeedFilter();

    $('#feed').scrollTop($('#feed')[0].scrollHeight);

    $('#feed-filter-menu a').on('click', function (e) {
        e.preventDefault();

        var mode = $(this).data('filter');

        try {
            localStorage.setItem('feedFilter', mode);
        } catch (err) {
            console.warn('Unable to access localStorage:', err);
        }

        $('#feed-filter-label').text($(this).text());
        applyFeedFilter();
        $('#feed').scrollTop($('#feed')[0].scrollHeight);
    });

     $('#chatForm').on('submit', function () {
        sendMessage();
    });

     $('#message').on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            sendMessage();
        }
    });

    $('#message').on('input', function() {
        var message = $('#message').val().trim();
        $('button[type="submit"]').prop('disabled', message.length === 0);
    });
});
