var skinsAdminPricesConfig = JSON.parse(document.getElementById("skins-admin-prices-config").textContent);

$(function () {
    var texts = {
        saved: skinsAdminPricesConfig.texts["Price saved. Reloading..."],
        invalid: skinsAdminPricesConfig.texts["Enter a price between 1 and 100,000."],
        error: skinsAdminPricesConfig.texts["Could not save. Reload the page and try again."]
    };

    var token = $('input[name="__RequestVerificationToken"]').val();
    var selected = null;

    function message(success, text) {
        Hub.alert($('#ska-price-modal .ska-message'), success, text);
    }

    function save(price) {
        var $buttons = $('#ska-price-modal .ska-save, #ska-price-modal .ska-reset');

        $buttons.prop('disabled', true);

        $.ajax({
            url: '/api/shop/skins/' + encodeURIComponent(selected) + '/price',
            type: 'POST',
            contentType: 'application/json',
            headers: { 'RequestVerificationToken': token },
            data: JSON.stringify({ price: price })
        }).done(function () {
            message(true, texts.saved);
            setTimeout(function () { location.reload(); }, 900);
        }).fail(function () {
            message(false, texts.error);
            $buttons.prop('disabled', false);
        });
    }

    $('.ska-skin').on('click', function () {
        var $item = $(this);
        var $modal = $('#ska-price-modal');

        selected = $item.data('skin');

        $modal.find('.ska-title').text($item.data('title'));
        $modal.find('.ska-image').attr({ src: $item.data('image'), alt: $item.data('title') });
        $modal.find('.ska-weapon').text($item.data('weapon'));
        $modal.find('.ska-rarity').text($item.data('rarity'));
        $modal.find('.ska-default-price').text(Number($item.data('default-price')).toLocaleString() + ' ⭐');
        $modal.find('.ska-price').val($item.data('price'));
        $modal.find('.ska-reset').toggle($item.data('custom') === 1);
        $modal.find('.ska-gift').toggle($item.data('available') === 1);
        $modal.find('.ska-message').empty();
        $modal.find('.ska-save, .ska-reset').prop('disabled', false);
        $modal.modal('show');
    });

    $('.ska-save').on('click', function () {
        var price = parseInt($('#ska-price-modal .ska-price').val(), 10);

        if (isNaN(price) || price < 1 || price > 100000) {
            message(false, texts.invalid);
            return;
        }

        save(price);
    });

    $('.ska-gift').on('click', function () {
        var skin = selected;

        $('#ska-price-modal').one('hidden.bs.modal', function () {
            GiftModal.open({ skin: skin });
        }).modal('hide');
    });

    $('.ska-reset').on('click', function () {
        if (confirm(skinsAdminPricesConfig.texts["Go back to the catalog price for this skin?"]))
            save(null);
    });

    $('#ska-price-modal').on('shown.bs.modal', function () {
        $(this).find('.ska-price').focus().select();
    });
});
