var HubPage = JSON.parse(document.getElementById("skins-admin-purchases-config").textContent);

$(function () {
    var texts = HubPage.texts;

    var token = $('input[name="__RequestVerificationToken"]').val();
    var purchasesUrl = HubPage.purchasesUrl;
    var query = HubPage.query;
    var playerPageUrl = HubPage.playerPageUrl;
    var rarityClasses = 'ska-rarity-legendary ska-rarity-epic ska-rarity-rare ska-rarity-uncommon ska-rarity-common';
    var selected = null;

    function playerUrl(communityId) {
        return purchasesUrl + '?' + $.param(query === '' ? { player: communityId } : { player: communityId, q: query });
    }

    function post(url, data) {
        return $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json',
            headers: { 'RequestVerificationToken': token },
            data: JSON.stringify(data)
        });
    }

    function errorText(xhr, response) {
        return texts.errors[response && response.status] || texts.errors.Generic;
    }

    $('.ska-purchase').on('click', function () {
        var $item = $(this);
        var $modal = $('#ska-purchase-modal');
        var gift = $item.data('gift') === 1;

        selected = { player: $item.data('player').toString(), name: $item.data('player-name'), skin: $item.data('skin'), title: $item.data('title') };

        var shopUrl = $item.data('shop-url');
        var rarity = $item.data('rarity');

        $modal.find('.modal-content').removeClass(rarityClasses).addClass($item.data('rarity-css'));
        $modal.find('.ska-image').attr({ src: $item.data('image'), alt: $item.data('title') });
        $modal.find('.ska-title').text($item.data('title'));
        $modal.find('.ska-shop-link').attr('href', shopUrl || '#').toggleClass('disabled', !shopUrl);
        $modal.find('.btn.ska-shop-link').toggle(!!shopUrl);
        $modal.find('.ska-cover-hint').toggle(!!shopUrl);
        $modal.find('.ska-rarity').text(rarity).toggle(!!rarity);
        $modal.find('.ska-gift-badge').toggle(gift);
        $modal.find('.ska-weapon').text([$item.data('weapon'), $item.data('skin')].filter(Boolean).join(' · '));
        $modal.find('.ska-avatar').attr({ src: $item.data('avatar'), alt: $item.data('player-name') });
        $modal.find('.ska-player-name').text($item.data('player-name'));
        $modal.find('.ska-player-link').attr('href', playerPageUrl + encodeURIComponent(selected.player));
        $modal.find('.ska-player-filter').attr('href', playerUrl(selected.player));
        $modal.find('.ska-player-id').text(selected.player);
        $modal.find('.ska-price').text($item.data('price'));
        $modal.find('.ska-stars-before').text($item.data('stars-before') + ' ⭐');
        $modal.find('.ska-balance-after').text('→ ' + $item.data('stars-after') + ' ⭐');
        $modal.find('.ska-stars-before-box').toggle(!gift);
        $modal.find('.ska-gifted-by').text($item.data('gifted-by'));
        $modal.find('.ska-gifted-by-box').toggle(gift && !!$item.data('gifted-by'));
        $modal.find('.ska-date').text($item.data('date'));
        $modal.find('.ska-refund-confirm').hide().text('');
        $modal.find('.ska-message').empty();
        $modal.find('.ska-refund').show().prop('disabled', false);
        $modal.find('.ska-refund-yes').hide().prop('disabled', false);
        $modal.modal('show');
    });

    $('.ska-shop-link').on('click', function (event) {
        if ($(this).hasClass('disabled'))
            event.preventDefault();
    });

    $('.ska-pick-player').on('click', function (event) {
        event.preventDefault();

        PlayerPicker.open(function (player) {
            if (player)
                location.href = playerUrl(player.communityId);
        });
    });

    $('.ska-refund').on('click', function () {
        var $modal = $('#ska-purchase-modal');

        $modal.find('.ska-refund-confirm').text(Hub.format(texts.confirmRefund, selected.title, selected.name)).show();
        $(this).hide();
        $modal.find('.ska-refund-yes').show();
    });

    $('.ska-refund-yes').on('click', function () {
        var $button = $(this);
        var $message = $('#ska-purchase-modal .ska-message');

        $button.prop('disabled', true);

        post('/api/shop/refund', { communityId: selected.player, skinId: selected.skin }).done(function (response) {
            if (!response.succeeded) {
                Hub.alert($message, false, errorText(null, response));
                $button.prop('disabled', false);
                return;
            }

            Hub.alert($message, true, texts.refundDone);
            setTimeout(function () { location.reload(); }, 1200);
        }).fail(function (xhr) {
            Hub.alert($message, false, errorText(xhr));
            $button.prop('disabled', false);
        });
    });

    function number(value) {
        return value.toLocaleString(document.documentElement.lang || undefined);
    }

    function resetRefundAll($modal) {
        $modal.find('.ska-refund-all-check, .ska-refund-all-close').prop('disabled', false);
        $modal.find('.ska-refund-all-label').text(texts.confirmRefundAll);
        $modal.find('.ska-refund-all-yes').prop('disabled', !$modal.find('.ska-refund-all-check').prop('checked'));
    }

    $('.ska-refund-all-open').on('click', function () {
        var $modal = $('#ska-refund-all-modal');

        $modal.find('.ska-refund-all-check').prop('checked', false);
        $modal.find('.ska-refund-all-message').empty();
        resetRefundAll($modal);
        $modal.modal('show');
    });

    $('.ska-refund-all-check').on('change', function () {
        $('#ska-refund-all-modal .ska-refund-all-yes').prop('disabled', !this.checked);
    });

    $('.ska-refund-all-yes').on('click', function () {
        var $modal = $('#ska-refund-all-modal');
        var $message = $modal.find('.ska-refund-all-message');

        $modal.find('.ska-refund-all-yes, .ska-refund-all-check, .ska-refund-all-close').prop('disabled', true);
        $modal.find('.ska-refund-all-label').text(texts.refundAllRunning);
        $message.empty();

        post('/api/shop/refund-all', {}).done(function (response) {
            var summary = Hub.format(texts.refundAllDone, number(response.purchases), number(response.gifts), number(response.stars), number(response.players));

            if (response.failed > 0) {
                Hub.alert($message, false, summary + ' ' + Hub.format(texts.refundAllFailed, number(response.failed)));
                resetRefundAll($modal);
                $modal.one('hidden.bs.modal', function () { location.reload(); });
                return;
            }

            Hub.alert($message, true, summary);
            setTimeout(function () { location.reload(); }, 2500);
        }).fail(function (xhr) {
            Hub.alert($message, false, errorText(xhr));
            resetRefundAll($modal);
        });
    });
});
