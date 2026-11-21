$(function () {
    var texts = HubPage.texts;

    var selected = null;
    var playerUrl = HubPage.playerUrl;
    var lookupUrl = HubPage.lookupUrl;
    var headers = { RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val() };

    function saveBan(communityId, name, reason) {
        return $.ajax({
            url: '/api/chat-bans',
            type: 'POST',
            headers: headers,
            contentType: 'application/json',
            data: JSON.stringify({ communityId: communityId, name: name || null, reason: reason || null })
        });
    }

    $('.cb-ban').on('click', function () {
        var $item = $(this);
        var $modal = $('#cb-details-modal');
        var updatedAt = $item.data('updated-at');
        var bannedBy = $item.data('banned-by');
        var profileUrl = $item.data('profile-url');

        selected = { communityId: $item.data('community-id').toString(), name: $item.data('name'), storedName: $item.attr('data-stored-name') };

        $modal.find('.cb-avatar').attr({ src: $item.data('avatar'), alt: selected.name });
        $modal.find('.cb-player-link').text(selected.name).attr('href', playerUrl + encodeURIComponent(selected.communityId));
        $modal.find('.cb-ids').text(($item.data('steam-id') || '') + ' · ' + selected.communityId);
        $modal.find('.cb-steam-link').attr('href', profileUrl || '#').toggle(!!profileUrl);
        $modal.find('.cb-banned-at').text($item.data('banned-at'));
        $modal.find('.cb-updated-at').text(updatedAt || '');
        $modal.find('.cb-updated-at, .cb-updated-at-label').toggle(!!updatedAt);
        $modal.find('.cb-banned-by').text(bannedBy || '');
        $modal.find('.cb-banned-by, .cb-banned-by-label').toggle(!!bannedBy);
        $('#cb-details-reason').val($item.data('reason') || '');
        $modal.find('.cb-remove-confirm').hide();
        $modal.find('.cb-message').empty();
        $modal.find('.cb-remove').show();
        $modal.find('.cb-remove-yes').hide();
        $modal.find('button').prop('disabled', false);
        $modal.modal('show');
    });

    $('.cb-save-reason').on('click', function () {
        var $button = $(this);
        var $message = $('#cb-details-modal .cb-message');

        $button.prop('disabled', true);

        saveBan(selected.communityId, selected.storedName || selected.name, $.trim($('#cb-details-reason').val())).done(function () {
            Hub.alert($message, true, texts.saved);
            setTimeout(function () { location.reload(); }, 900);
        }).fail(function (xhr) {
            Hub.alert($message, false, Hub.errorText(xhr, texts.error));
            $button.prop('disabled', false);
        });
    });

    $('.cb-remove').on('click', function () {
        $('#cb-details-modal .cb-remove-confirm').show();
        $(this).hide();
        $('#cb-details-modal .cb-remove-yes').show();
    });

    $('.cb-remove-yes').on('click', function () {
        var $button = $(this);
        var $message = $('#cb-details-modal .cb-message');

        $button.prop('disabled', true);

        $.ajax({
            url: '/api/chat-bans/' + encodeURIComponent(selected.communityId),
            type: 'DELETE',
            headers: headers
        }).done(function () {
            Hub.alert($message, true, texts.removed);
            setTimeout(function () { location.reload(); }, 900);
        }).fail(function (xhr) {
            Hub.alert($message, false, Hub.errorText(xhr, texts.error));
            $button.prop('disabled', false);
        });
    });

    function loadRecentSenders() {
        var $list = $('#cb-add-modal .cb-recent').empty();

        $.getJSON('/api/chat-bans/recent-senders').done(function (senders) {
            if (!senders || senders.length === 0) {
                $list.append('<div class="list-group-item text-muted">' + Hub.escapeHtml(texts.noRecent) + '</div>');
                return;
            }

            $.each(senders, function (index, sender) {
                var $item = $('<a href="#" class="list-group-item"></a>');

                $item.append('<img src="' + Hub.escapeHtml(sender.avatarUrl) + '" alt="" style="width: 24px; border-radius: 50%; margin-right: 8px" onerror="imgOnError(this)"/>');
                $item.append($('<strong></strong>').text(sender.name || sender.communityId));

                if (sender.alreadyBanned) {
                    $item.addClass('disabled');
                    $item.append(' <span class="label label-default">' + Hub.escapeHtml(texts.alreadyBanned) + '</span>');
                }

                $item.append($('<div class="text-muted small"></div>').text(sender.message));

                $item.on('click', function (event) {
                    event.preventDefault();

                    if (sender.alreadyBanned)
                        return;

                    lookupSequence++;
                    $('#cb-add-player').val(sender.communityId);
                    $('#cb-add-name').val(sender.name || '');
                    $('#cb-add-modal .cb-picked').text(sender.name || sender.communityId);
                    $('#cb-add-reason').focus();
                });

                $list.append($item);
            });
        });
    }

    $('.cb-add').on('click', function () {
        $('#cb-add-player').val('');
        $('#cb-add-name').val('');
        $('#cb-add-reason').val('');
        $('#cb-add-modal .cb-picked').empty();
        $('#cb-add-modal .cb-add-message').empty();
        $('#cb-add-modal .cb-add-save').prop('disabled', false);
        loadRecentSenders();
        $('#cb-add-modal').modal('show');
    });

    $('.cb-pick-player').on('click', function () {
        PlayerPicker.swap($('#cb-add-modal'), function (player) {
            lookupSequence++;
            $('#cb-add-player').val(player.communityId);
            $('#cb-add-name').val(player.name || '');
            $('#cb-add-modal .cb-picked').html('<img src="' + Hub.escapeHtml(player.avatarUrl) + '" alt="" style="width: 24px; border-radius: 50%; margin-right: 6px" onerror="imgOnError(this)"/>' + Hub.escapeHtml(player.name));
        });
    });

    var lookupTimer = null;
    var lookupSequence = 0;

    $('#cb-add-player').on('input', function () {
        var value = $.trim($(this).val());
        var sequence = ++lookupSequence;

        $('#cb-add-name').val('');
        $('#cb-add-modal .cb-picked').empty();
        clearTimeout(lookupTimer);

        if (value === '')
            return;

        lookupTimer = setTimeout(function () {
            $.getJSON(lookupUrl, { id: value }).done(function (player) {
                if (sequence !== lookupSequence || !player)
                    return;

                $('#cb-add-name').val(player.name || '');
                $('#cb-add-modal .cb-picked').html('<img src="' + Hub.escapeHtml(player.avatarUrl) + '" alt="" style="width: 24px; border-radius: 50%; margin-right: 6px" onerror="imgOnError(this)"/>' + Hub.escapeHtml(player.name || player.communityId));
            });
        }, 400);
    });

    $('.cb-add-save').on('click', function () {
        var $button = $(this);
        var $message = $('#cb-add-modal .cb-add-message');
        var player = $.trim($('#cb-add-player').val());

        if (player === '') {
            Hub.alert($message, false, texts.required);
            return;
        }

        $button.prop('disabled', true);

        saveBan(player, $('#cb-add-name').val(), $.trim($('#cb-add-reason').val())).done(function () {
            Hub.alert($message, true, texts.saved);
            setTimeout(function () { location.reload(); }, 900);
        }).fail(function (xhr) {
            Hub.alert($message, false, Hub.errorText(xhr, texts.error));
            $button.prop('disabled', false);
        });
    });
});
