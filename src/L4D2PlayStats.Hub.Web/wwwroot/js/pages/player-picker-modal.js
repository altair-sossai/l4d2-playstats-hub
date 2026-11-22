var playerPickerModalConfig = JSON.parse(document.getElementById("player-picker-modal-config").textContent);

document.addEventListener('DOMContentLoaded', function () {
    var texts = {
        loading: playerPickerModalConfig.texts["Loading players..."],
        empty: playerPickerModalConfig.texts["No players found."],
        error: playerPickerModalConfig.texts["Could not load the players. Close and try again."],
        history: playerPickerModalConfig.texts["Shared.PlayerPickerModal.FromHistory"],
        alsoKnownAs: playerPickerModalConfig.texts["Also known as:"],
        lastSeen: playerPickerModalConfig.texts["Last seen"],
        neverSeen: playerPickerModalConfig.texts["Never seen on the servers"],
        unknown: playerPickerModalConfig.texts["Unknown player"],
        count: playerPickerModalConfig.texts["{0} of {1} players"]
    };

    var searchUrl = playerPickerModalConfig.values.playerSearchUrl;
    var delay = playerPickerModalConfig.values.playerSearchDelayInMilliseconds;
    var $modal = $('#pp-modal');
    var $search = $modal.find('.pp-search');
    var $list = $modal.find('.pp-list');
    var $more = $modal.find('.pp-more');
    var $count = $modal.find('.pp-count');
    var $hint = $modal.find('.pp-hint');
    var defaultHint = $hint.text();
    var source = searchUrl;
    var extra = {};
    var callback = null;
    var chosen = null;
    var term = '';
    var page = 0;
    var sequence = 0;
    var timer = null;

    function note(text) {
        $list.html('<div class="list-group-item pp-note text-muted">' + Hub.escapeHtml(text) + '</div>');
    }

    function item(player) {
        var $item = $('<a href="#" class="list-group-item pp-player"></a>');
        var seen = player.lastSeen ? (player.lastSeenLabel || texts.lastSeen) + ': ' + player.lastSeen : texts.neverSeen;
        var aliases = (player.previousNames || []).length > 0 ? '<small>' + Hub.escapeHtml(texts.alsoKnownAs + ' ' + player.previousNames.join(', ')) + '</small>' : '';
        var badge = player.position ? '#' + player.position : (player.ranked ? texts.history : '');

        $item.attr('data-community-id', player.communityId);
        $item.append('<img src="' + Hub.escapeHtml(player.avatarUrl) + '" alt="" onerror="imgOnError(this)"/>');
        $item.append('<div class="pp-info"><strong>' + Hub.escapeHtml(player.name || texts.unknown) + '</strong><small class="pp-meta"><span title="SteamID64: ' + Hub.escapeHtml(player.communityId) + '">' + Hub.escapeHtml(player.steamId) + '</span> · ' + Hub.escapeHtml(seen) + '</small>' + aliases + '</div>');

        if (badge !== '')
            $item.append('<span class="label label-default">' + Hub.escapeHtml(badge) + '</span>');

        $item.on('click', function (event) {
            event.preventDefault();
            chosen = player;
            $modal.modal('hide');
        });

        return $item;
    }

    function load(reset) {
        var current = ++sequence;

        if (reset) {
            page = 0;
            $more.hide();
            $count.text('');
            note(texts.loading);
        }

        $more.prop('disabled', true);

        $.getJSON(source, $.extend({}, extra, { q: term, page: page + 1 })).done(function (result) {
            if (current !== sequence)
                return;

            if (reset)
                $list.empty();

            page = result.page;

            $.each(result.players || [], function (index, player) {
                $list.append(item(player));
            });

            var shown = $list.children('.pp-player').length;

            if (shown === 0)
                note(texts.empty);

            $count.text(shown > 0 ? Hub.format(texts.count, shown, result.total) : '');
            $more.toggle(!!result.hasMore).prop('disabled', false);
        }).fail(function () {
            if (current !== sequence)
                return;

            $more.hide();

            if (reset)
                note(texts.error);
            else
                $list.append('<div class="list-group-item pp-note text-danger">' + Hub.escapeHtml(texts.error) + '</div>');
        });
    }

    function search() {
        clearTimeout(timer);
        term = $.trim($search.val());
        load(true);
    }

    $search.on('input', function () {
        clearTimeout(timer);
        timer = setTimeout(search, delay);
    });

    $search.on('keydown', function (event) {
        if (event.key !== 'Enter')
            return;

        event.preventDefault();
        search();
    });

    $more.on('click', function () {
        load(false);
    });

    $modal.on('shown.bs.modal', function () {
        $search.focus();
    });

    $modal.on('hidden.bs.modal', function () {
        var done = callback;
        var player = chosen;

        clearTimeout(timer);
        sequence++;
        callback = null;
        chosen = null;

        if (done)
            done(player);
    });

    window.PlayerPicker = {
        open: function (onClose, options) {
            source = (options && options.url) || searchUrl;
            extra = (options && options.params) || {};
            $hint.text((options && options.hint) || defaultHint);
            callback = onClose;
            chosen = null;
            term = '';
            $search.val('');
            $modal.modal('show');
            load(true);
        },
        swap: function ($parent, onPick, options, onClose) {
            function pick() {
                window.PlayerPicker.open(function (player) {
                    if (player)
                        onPick(player);

                    if ($parent.length > 0)
                        $parent.modal('show');

                    if (onClose)
                        onClose(player);
                }, options);
            }

            if ($parent.length > 0)
                $parent.one('hidden.bs.modal', pick).modal('hide');
            else
                pick();
        }
    };

    $(document).on('click', '[data-player-picker]', function (event) {
        event.preventDefault();

        var $button = $(this);
        var $target = $($button.attr('data-player-picker'));
        var $name = $($button.attr('data-player-picker-name') || []);
        var $parent = $button.closest('.modal');
        var options = { hint: $button.attr('data-player-picker-hint'), url: $button.attr('data-player-picker-source'), params: $button.attr('data-player-picker-role') ? { role: $button.attr('data-player-picker-role') } : {} };

        window.PlayerPicker.swap($parent, function (player) {
            $target.val(player.communityId).trigger('input').trigger('change');

            if ($name.length > 0 && $.trim($name.val()) === '' && player.name)
                $name.val(player.name).trigger('input');

            var form = $target.closest('form')[0];

            if (form && $button.is('[data-player-picker-submit]')) {
                if (form.requestSubmit)
                    form.requestSubmit();
                else
                    form.submit();
            }
        }, options, function () {
            if ($parent.length === 0 && $target.is(':visible'))
                $target.focus();
        });
    });
});
