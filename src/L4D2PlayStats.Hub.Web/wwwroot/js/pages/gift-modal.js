var giftModalConfig = JSON.parse(document.getElementById("gift-modal-config").textContent);

document.addEventListener('DOMContentLoaded', function () {
    var texts = {
        noPlayer: giftModalConfig.texts["No player chosen"],
        noSkin: giftModalConfig.texts["No skin chosen"],
        requiredPlayer: giftModalConfig.texts["Choose the player or enter the Steam ID."],
        requiredSkin: giftModalConfig.texts["Choose the skin."],
        done: giftModalConfig.texts["Gift given! Reloading..."],
        typed: giftModalConfig.texts["Steam ID typed manually"],
        errors: {
            AlreadyOwned: giftModalConfig.texts["The player already owns this skin."],
            SkinNotFound: giftModalConfig.texts["Skin not found."],
            Busy: giftModalConfig.texts["Too many operations at the same time. Try again."],
            BadRequest: giftModalConfig.texts["Invalid Steam ID. Use STEAM_0:1:123, [U:1:123], 7656119... or the profile link."],
            Generic: giftModalConfig.texts["Something went wrong. Reload the page and check before trying again."]
        }
    };

    var $modal = $('#gm-modal');
    var skin = null;

    function message(success, text) {
        Hub.alert($modal.find('.gm-message'), success, text);
    }

    function showPlayer(player) {
        var $choice = $modal.find('.gm-player');

        if (!player) {
            $choice.html('<span class="gm-empty">' + Hub.escapeHtml(texts.noPlayer) + '</span>');
            return;
        }

        $choice.html('<img class="gm-avatar" src="' + Hub.escapeHtml(player.avatarUrl) + '" alt="" onerror="imgOnError(this)"/>'
            + '<div><strong>' + Hub.escapeHtml(player.name) + '</strong><small>' + Hub.escapeHtml(player.steamId || '') + ' · ' + Hub.escapeHtml(player.communityId) + '</small></div>');
    }

    function showSkin(selected) {
        skin = selected;

        var $choice = $modal.find('.gm-skin');

        if (!skin) {
            $choice.html('<span class="gm-empty">' + Hub.escapeHtml(texts.noSkin) + '</span>');
            return;
        }

        $choice.html('<img class="gm-thumb" src="' + Hub.escapeHtml(skin.image) + '" alt=""/>'
            + '<div><strong>' + Hub.escapeHtml(skin.title) + '</strong><small>' + Hub.escapeHtml(skin.weaponName) + ' · ' + Hub.escapeHtml(skin.rarityName) + ' · ' + Number(skin.price).toLocaleString() + ' ⭐</small></div>');
    }

    function swap(openPicker) {
        $modal.one('hidden.bs.modal', openPicker).modal('hide');
    }

    $modal.find('.gm-pick-player').on('click', function () {
        window.PlayerPicker.swap($modal, function (player) {
            $modal.find('.gm-player-id').val(player.communityId);
            showPlayer(player);
        });
    });

    $modal.find('.gm-player-id').on('input', function () {
        var value = $.trim($(this).val());

        $modal.find('.gm-player').html('<span class="gm-empty">' + Hub.escapeHtml(value === '' ? texts.noPlayer : texts.typed) + '</span>');
    });

    $modal.find('.gm-pick-skin').on('click', function () {
        swap(function () {
            window.SkinPicker.open(function (selected) {
                if (selected)
                    showSkin(selected);

                $modal.modal('show');
            });
        });
    });

    $modal.find('.gm-confirm').on('click', function () {
        var $button = $(this);
        var player = $.trim($modal.find('.gm-player-id').val());

        if (player === '') {
            message(false, texts.requiredPlayer);
            return;
        }

        if (!skin) {
            message(false, texts.requiredSkin);
            return;
        }

        $button.prop('disabled', true);

        $.ajax({
            url: '/api/shop/gift',
            type: 'POST',
            contentType: 'application/json',
            headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            data: JSON.stringify({ communityId: player, skinId: skin.id })
        }).done(function (response) {
            if (!response.succeeded) {
                message(false, texts.errors[response.status] || texts.errors.Generic);
                $button.prop('disabled', false);
                return;
            }

            message(true, texts.done);
            setTimeout(function () { location.reload(); }, 1500);
        }).fail(function (xhr) {
            message(false, xhr.status === 400 ? texts.errors.BadRequest : texts.errors.Generic);
            $button.prop('disabled', false);
        });
    });

    window.GiftModal = {
        open: function (options) {
            var settings = options || {};

            $modal.find('.gm-player-id').val('');
            $modal.find('.gm-message').empty();
            $modal.find('.gm-confirm').prop('disabled', false);
            showPlayer(null);
            showSkin(settings.skin ? window.SkinPicker.find(settings.skin) : null);
            $modal.modal('show');
        }
    };
});
