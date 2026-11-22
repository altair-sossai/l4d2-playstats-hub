var antiCheatAdminIndexConfig = JSON.parse(document.getElementById("anti-cheat-admin-index-config").textContent);

$(function () {
    var shownList = antiCheatAdminIndexConfig.shownList;
    var savedMode = antiCheatAdminIndexConfig.savedMode;
    var pageUrl = antiCheatAdminIndexConfig.pageUrl;
    var apiUrl = antiCheatAdminIndexConfig.apiUrl;
    var lookupUrl = antiCheatAdminIndexConfig.lookupUrl;
    var playerUrl = antiCheatAdminIndexConfig.playerUrl;
    var headers = { RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val() };
    var texts = antiCheatAdminIndexConfig.texts;
    var listedIds = antiCheatAdminIndexConfig.listedIds;

    var selected = null;
    var picked = null;
    var lookupTimer = null;
    var lookupSequence = 0;

    $('input[name="aca-mode"]').on('change', function () {
        var mode = $('input[name="aca-mode"]:checked').val();

        $('.aca-mode').removeClass('aca-selected');
        $(this).closest('.aca-mode').addClass('aca-selected');
        $('.aca-save-mode').prop('disabled', mode === savedMode);
    });

    $('.aca-save-mode').on('click', function () {
        var $button = $(this);
        var mode = $('input[name="aca-mode"]:checked').val();

        $button.prop('disabled', true);

        $.ajax({
            url: apiUrl + '/mode',
            type: 'PUT',
            headers: headers,
            contentType: 'application/json',
            data: JSON.stringify({ mode: mode })
        }).done(function () {
            Hub.alert($('.aca-mode-message'), true, texts.modeSaved);
            setTimeout(function () { location.href = pageUrl; }, 900);
        }).fail(function (xhr) {
            Hub.alert($('.aca-mode-message'), false, Hub.errorText(xhr, texts.error));
            $button.prop('disabled', false);
        });
    });

    $('.aca-player').on('click', function () {
        var $item = $(this);
        var $modal = $('#aca-details-modal');
        var profileUrl = $item.data('profile-url');

        selected = { communityId: $item.data('community-id').toString(), name: $item.data('name') };

        $modal.find('.aca-avatar').attr({ src: $item.data('avatar'), alt: selected.name });
        $modal.find('.aca-player-link').text(selected.name).attr('href', playerUrl + encodeURIComponent(selected.communityId));
        $modal.find('.aca-ids').text(($item.data('steam-id') || '') + ' · ' + selected.communityId);
        $modal.find('.aca-steam-link').attr('href', profileUrl || '#').toggle(!!profileUrl);
        $modal.find('.aca-added-at').text($item.data('added-at'));
        $modal.find('.aca-added-by').text($item.data('added-by'));
        $modal.find('.aca-remove-confirm').hide();
        $modal.find('.aca-message').empty();
        $modal.find('.aca-remove').show();
        $modal.find('.aca-remove-yes').hide();
        $modal.find('button').prop('disabled', false);
        $modal.modal('show');
    });

    $('.aca-remove').on('click', function () {
        $('#aca-details-modal .aca-remove-confirm').show();
        $(this).hide();
        $('#aca-details-modal .aca-remove-yes').show();
    });

    $('.aca-remove-yes').on('click', function () {
        var $button = $(this);
        var $message = $('#aca-details-modal .aca-message');

        $button.prop('disabled', true);

        $.ajax({
            url: apiUrl + '/' + shownList + '/' + encodeURIComponent(selected.communityId),
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

    function showPreview(player) {
        var $preview = $('#aca-add-modal .aca-preview').empty();

        if (!player)
            return;

        var origin = player.knownOnSite ? texts.known : texts.steam;
        var $info = $('<div></div>');

        $info.append($('<strong></strong>').text(player.name || texts.noName));
        $info.append($('<small></small>').text((player.steamId || '') + ' · ' + player.communityId));
        $info.append($('<small></small>').text(origin));

        $preview.append($('<img alt="" onerror="imgOnError(this)"/>').attr('src', player.avatarUrl || ''));
        $preview.append($info);
    }

    function setPicked(player) {
        picked = player;
        showPreview(player);

        var already = !!player && listedIds.indexOf(player.communityId) >= 0;

        $('#aca-add-modal .aca-add-message').empty();
        if (already)
            Hub.alert($('#aca-add-modal .aca-add-message'), false, texts.already);

        $('#aca-add-modal .aca-add-save').prop('disabled', !player || already);
    }

    function lookup(value) {
        var sequence = ++lookupSequence;
        var $preview = $('#aca-add-modal .aca-preview');

        if ($.trim(value) === '') {
            setPicked(null);
            return;
        }

        $preview.html('<span class="text-muted">' + Hub.escapeHtml(texts.searching) + '</span>');

        $.getJSON(lookupUrl, { id: $.trim(value) }).done(function (player) {
            if (sequence === lookupSequence)
                setPicked(player);
        }).fail(function () {
            if (sequence !== lookupSequence)
                return;

            setPicked(null);
            $preview.html('<span class="text-danger">' + Hub.escapeHtml(texts.notFound) + '</span>');
        });
    }

    $('.aca-add').on('click', function () {
        $('#aca-add-player').val('');
        setPicked(null);
        $('#aca-add-modal').modal('show');
    });

    $('#aca-add-modal').on('shown.bs.modal', function () {
        $('#aca-add-player').focus();
    });

    $('#aca-add-player').on('input', function () {
        var value = $(this).val();

        $('#aca-add-modal .aca-add-save').prop('disabled', true);
        clearTimeout(lookupTimer);
        lookupTimer = setTimeout(function () { lookup(value); }, 400);
    });

    $('.aca-pick-player').on('click', function () {
        PlayerPicker.swap($('#aca-add-modal'), function (player) {
            lookupSequence++;
            clearTimeout(lookupTimer);
            $('#aca-add-player').val(player.communityId);
            setPicked({
                communityId: player.communityId,
                steamId: player.steamId,
                name: player.name,
                avatarUrl: player.avatarUrl,
                knownOnSite: player.knownOnSite !== false
            });
        });
    });

    $('.aca-add-save').on('click', function () {
        var $button = $(this);
        var $message = $('#aca-add-modal .aca-add-message');

        if (!picked)
            return;

        $button.prop('disabled', true);

        $.ajax({
            url: apiUrl + '/' + shownList,
            type: 'POST',
            headers: headers,
            contentType: 'application/json',
            data: JSON.stringify({ player: picked.communityId, name: picked.name || null })
        }).done(function () {
            Hub.alert($message, true, texts.saved);
            setTimeout(function () { location.reload(); }, 900);
        }).fail(function (xhr) {
            Hub.alert($message, false, Hub.errorText(xhr, texts.error));
            $button.prop('disabled', false);
        });
    });
});
