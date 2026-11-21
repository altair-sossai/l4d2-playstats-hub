document.addEventListener('DOMContentLoaded', function () {
    var $modal = $('#sp-modal');
    var callback = null;
    var chosen = null;

    function filter() {
        var term = $.trim($modal.find('.sp-search').val()).toLowerCase();
        var weapon = $modal.find('.sp-weapon').val();
        var rarity = $modal.find('.sp-rarity-filter').val();
        var visible = 0;

        $modal.find('.sp-tile').each(function () {
            var $tile = $(this);
            var show = (term === '' || $tile.data('search').indexOf(term) >= 0)
                && (weapon === '' || $tile.data('weapon') === weapon)
                && (rarity === '' || $tile.data('rarity') === rarity);

            $tile.toggle(show);

            if (show)
                visible++;
        });

        $modal.find('.sp-empty').toggle(visible === 0);
    }

    $modal.find('.sp-search').on('input', filter);
    $modal.find('.sp-weapon, .sp-rarity-filter').on('change', filter);

    $modal.find('.sp-tile').on('click', function () {
        var $tile = $(this);

        chosen = {
            id: $tile.data('id'),
            title: $tile.data('title'),
            image: $tile.data('image'),
            weaponName: $tile.data('weapon-name'),
            rarityName: $tile.data('rarity-name'),
            price: $tile.data('price')
        };

        $modal.modal('hide');
    });

    $modal.on('shown.bs.modal', function () {
        $modal.find('.sp-search').focus();
    });

    $modal.on('hidden.bs.modal', function () {
        var done = callback;
        var skin = chosen;

        callback = null;
        chosen = null;

        if (done)
            done(skin);
    });

    window.SkinPicker = {
        find: function (id) {
            var $tile = $modal.find('.sp-tile[data-id="' + id + '"]');

            if ($tile.length === 0)
                return null;

            return {
                id: $tile.data('id'),
                title: $tile.data('title'),
                image: $tile.data('image'),
                weaponName: $tile.data('weapon-name'),
                rarityName: $tile.data('rarity-name'),
                price: $tile.data('price')
            };
        },
        open: function (onClose) {
            callback = onClose;
            chosen = null;
            $modal.find('.sp-search').val('');
            $modal.find('.sp-weapon, .sp-rarity-filter').val('');
            filter();
            $modal.modal('show');
        }
    };
});
