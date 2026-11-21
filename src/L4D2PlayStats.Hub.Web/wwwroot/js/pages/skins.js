$(function () {
    var texts = HubPage.texts;

    var token = $('input[name="__RequestVerificationToken"]').val();
    var weapon = 'all';
    var rarity = 'all';
    var onlyAffordable = false;
    var pendingBuy = null;
    var reloadOnClose = false;

    function number(value) {
        return Number(value).toLocaleString(document.documentElement.lang || undefined);
    }

    function showResult(title, html, reload) {
        var $result = $('#sk-result');

        reloadOnClose = reload;

        $result.find('.modal-title').text(title);
        $result.find('.modal-body').html(html);
        $('.modal.in').not($result).modal('hide');
        $result.modal('show');
    }

    function showError(status) {
        var message = texts.errors[status] || texts.errors.Generic;
        var reload = status === 'AlreadyOwned';

        showResult(texts.errorTitle, '<p>' + message + '</p>', reload);
    }

    function post(url, data, $button, onSuccess) {
        $button.prop('disabled', true);

        $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json',
            headers: { 'RequestVerificationToken': token },
            data: JSON.stringify(data || {})
        }).done(function (response) {
            if (response.succeeded) {
                onSuccess(response);
                return;
            }

            showError(response.status);
        }).fail(function () {
            showError('Generic');
        }).always(function () {
            $button.prop('disabled', false);
        });
    }

    var shotExtension = 'webp';
    var cardShots = [1, 5, 6];
    var cardShotDelay = 1600;
    var avifProbe = new Image();

    avifProbe.onload = function () {
        if (avifProbe.width > 0)
            shotExtension = 'avif';
    };
    avifProbe.src = 'data:image/avif;base64,AAAAIGZ0eXBhdmlmAAAAAGF2aWZtaWYxbWlhZk1BMUIAAAD5bWV0YQAAAAAAAAAvaGRscgAAAAAAAAAAcGljdAAAAAAAAAAAAAAAAFBpY3R1cmVIYW5kbGVyAAAAAA5waXRtAAAAAAABAAAAHmlsb2MAAAAARAAAAQABAAAAAQAAASEAAAAWAAAAKGlpbmYAAAAAAAEAAAAaaW5mZQIAAAAAAQAAYXYwMUNvbG9yAAAAAGppcHJwAAAAS2lwY28AAAAUaXNwZQAAAAAAAAACAAAAAgAAABBwaXhpAAAAAAMICAgAAAAMYXYxQ4EADAAAAAATY29scm5jbHgAAgACAAIAAAAAF2lwbWEAAAAAAAAAAQABBAECgwQAAAAebWRhdAoFGAA2wCAyDRgAAABQAAAAALATSyg=';

    function shotUrl(base, version, shot, size, extension) {
        return base + shot + size + '.' + (extension || shotExtension) + '?v=' + version;
    }

    function startCardShots($thumb) {
        var $shots = $thumb.children('.sk-shot');
        var index = 0;

        if ($shots.length === 0) {
            $shots = $($.map(cardShots, function (shot) {
                return $('<img class="sk-shot" alt=""/>').attr('src', shotUrl($thumb.data('shots'), $thumb.data('version'), shot, '-card'))[0];
            }));
            $thumb.children('img').first().after($shots);
        }

        function show() {
            var $next = $shots.eq(index % $shots.length);

            function reveal() {
                if (!$thumb.data('hovering'))
                    return;

                $shots.css('z-index', 0);
                $next.css('z-index', 1).addClass('active');
                setTimeout(function () {
                    if ($next.css('z-index') === '1')
                        $shots.not($next).removeClass('active');
                }, 500);
            }

            index++;

            if ($next[0].complete && $next[0].naturalWidth > 0)
                reveal();
            else
                $next.one('load', reveal);
        }

        $thumb.data('hovering', true);
        show();
        $thumb.data('timer', setInterval(show, cardShotDelay));
    }

    function stopCardShots($thumb) {
        clearInterval($thumb.data('timer'));
        $thumb.data('hovering', false);
        $thumb.children('.sk-shot').removeClass('active');
    }

    $(document).on('mouseenter', '.sk-thumb[data-shots]', function () {
        startCardShots($(this));
    });

    $(document).on('mouseleave', '.sk-thumb[data-shots]', function () {
        stopCardShots($(this));
    });

    function loadGallery($container) {
        $container.find('.sk-gallery').each(function () {
            var $gallery = $(this);

            $gallery.find('img[data-shot]').each(function () {
                var $image = $(this);

                $image.attr('src', shotUrl($gallery.data('shots'), $gallery.data('version'), $image.data('shot'), $image.attr('data-size')));
            });
        });
    }

    $(document).on('click', '.sk-gallery-thumb', function () {
        var $thumb = $(this);
        var $gallery = $thumb.closest('.sk-gallery');
        var $main = $gallery.find('.sk-gallery-main');
        var $image = $main.find('img');
        var shot = $thumb.data('shot');
        var base = $gallery.data('shots');
        var version = $gallery.data('version');

        if ($thumb.hasClass('active'))
            return;

        $gallery.find('.sk-gallery-thumb').removeClass('active');
        $thumb.addClass('active');
        $main.attr('href', shotUrl(base, version, shot, '', 'webp'));
        $image.addClass('loading').one('load error', function () {
            $image.removeClass('loading');
        }).attr('src', shotUrl(base, version, shot, ''));
    });

    var zoomMaxScale = 4;
    var zoomStep = 0.7;
    var zoomClickScale = 2;

    $(document).on('lity:ready', function (event, instance) {
        var $opener = instance.opener();
        var lity = instance.element()[0];
        var image = instance.element().find('.lity-content img')[0];
        var panned = false;

        if (!image || !$opener || !$opener.is('.sk-modal-image'))
            return;

        var panzoom = Panzoom(image, { maxScale: zoomMaxScale, step: zoomStep, contain: 'outside', cursor: 'zoom-in' });

        image.parentElement.addEventListener('wheel', panzoom.zoomWithWheel);
        lity.addEventListener('pointerdown', function () {
            panned = false;
        }, true);
        lity.addEventListener('click', function (e) {
            if (panned)
                e.stopPropagation();
        }, true);
        image.addEventListener('panzoompan', function () {
            panned = true;
        });
        image.addEventListener('panzoomchange', function (e) {
            image.style.cursor = e.detail.scale > 1 ? 'grab' : 'zoom-in';
        });
        image.addEventListener('click', function (e) {
            if (panzoom.getScale() > 1)
                panzoom.reset();
            else
                panzoom.zoomToPoint(zoomClickScale, e);
        });

        var $thumbs = $opener.closest('.sk-gallery').find('.sk-gallery-thumb');

        if ($thumbs.length < 2)
            return;

        function showShot(offset) {
            var index = ($thumbs.index($thumbs.filter('.active')) + offset + $thumbs.length) % $thumbs.length;

            $thumbs.eq(index).trigger('click');
            panzoom.reset({ animate: false });
            image.src = $opener.attr('href');
        }

        function onKeyDown(e) {
            if (e.key === 'ArrowLeft')
                showShot(-1);
            else if (e.key === 'ArrowRight')
                showShot(1);
            else
                return;

            e.preventDefault();
        }

        $('<button type="button" class="sk-lity-nav sk-lity-prev"><span class="glyphicon glyphicon-chevron-left"></span></button>')
            .attr('aria-label', texts.previousPhoto)
            .on('click', function () {
                showShot(-1);
            })
            .appendTo(lity);

        $('<button type="button" class="sk-lity-nav sk-lity-next"><span class="glyphicon glyphicon-chevron-right"></span></button>')
            .attr('aria-label', texts.nextPhoto)
            .on('click', function () {
                showShot(1);
            })
            .appendTo(lity);

        document.addEventListener('keydown', onKeyDown);
        instance.element().on('lity:close', function () {
            document.removeEventListener('keydown', onKeyDown);
        });
    });

    function openDetails($card) {
        var $modal = $('#sk-modal');

        $modal.find('.modal-title').text($card.data('title'));
        $modal.find('.modal-body').html($card.find('.sk-details').html());
        loadGallery($modal);
        $modal.data('skin', $card.data('skin'));
        $modal.data('title', $card.data('title'));
        $modal.data('workshop', $card.data('workshop'));
        $modal.modal('show');
    }

    $(document).on('click', '.sk-author-notice-open', function () {
        var $modal = $('#sk-modal');
        var title = 'Remove skin: ' + $modal.data('title') + ' (' + $modal.data('skin') + ')';
        var body = 'Skin: ' + $modal.data('title') + '\nWorkshop: ' + $modal.data('workshop');

        if (texts.authorRequestUrl)
            $('#sk-author-notice .sk-author-notice-github').attr('href',
                texts.authorRequestUrl + (texts.authorRequestUrl.indexOf('?') < 0 ? '?' : '&') + 'title=' + encodeURIComponent(title) + '&body=' + encodeURIComponent(body));
        $('#sk-author-notice').modal('show');
    });

    $('#sk-author-notice').on('show.bs.modal', function () {
        var zIndex = 1050 + 10 * $('.modal.in').length;

        $(this).css('z-index', zIndex);
        setTimeout(function () {
            $('.modal-backdrop').not('.sk-stacked').last().css('z-index', zIndex - 1).addClass('sk-stacked');
        }, 0);
    }).on('hidden.bs.modal', function () {
        if ($('.modal.in').length > 0)
            $('body').addClass('modal-open');
    });

    $(document).on('click', '.sk-open', function (e) {
        if ($(e.target).closest('.sk-action, .sk-missing, .sk-author').length > 0)
            return;

        e.preventDefault();
        openDetails($(this));
    });

    $(document).on('click', '.sk-buy', function (e) {
        e.preventDefault();
        e.stopPropagation();

        var $button = $(this);
        var price = Number($button.data('price'));
        var title = Hub.escapeHtml($button.data('title'));
        var weaponName = Hub.escapeHtml($button.data('weapon-name'));

        pendingBuy = { skin: $button.data('skin'), price: price, title: title, weaponName: weaponName };

        var html = '<p>' + Hub.format(texts.confirmBuy, title, number(price)) + '</p>'
            + '<p>' + Hub.format(texts.confirmBalance, number(texts.balance), number(texts.balance - price)) + '</p>'
            + '<p class="sk-notice">' + Hub.format(texts.confirmEquip, weaponName) + '</p>'
            + (texts.confirmSeason ? '<p>' + Hub.escapeHtml(texts.confirmSeason) + '</p>' : '')
            + '<p><small>' + texts.confirmFinal + '</small></p>';

        $('.modal.in').modal('hide');
        $('#sk-confirm .modal-body').html(html);
        $('#sk-confirm').modal('show');
    });

    $('.sk-confirm-buy').on('click', function () {
        if (pendingBuy == null)
            return;

        var buy = pendingBuy;

        post('/api/shop/skins/' + encodeURIComponent(buy.skin) + '/buy', null, $(this), function () {
            pendingBuy = null;

            var html = '<p>' + Hub.format(texts.buySuccess, buy.title, buy.weaponName) + '</p>'
                + '<p class="sk-notice">' + texts.nextMap + '</p>'
                + '<p>' + texts.switchHint + '</p>';

            showResult(texts.buySuccessTitle, html, true);
        });
    });

    $(document).on('click', '.sk-equip', function (e) {
        e.preventDefault();
        e.stopPropagation();

        var $button = $(this);
        var title = Hub.escapeHtml($button.data('title'));
        var weaponName = Hub.escapeHtml($button.data('weapon-name'));

        post('/api/shop/skins/' + encodeURIComponent($button.data('skin')) + '/equip', null, $button, function () {
            var html = '<p>' + Hub.format(texts.equipSuccess, title, weaponName) + '</p>'
                + '<p class="sk-notice">' + texts.nextMap + '</p>';

            showResult(texts.equipSuccessTitle, html, true);
        });
    });

    $(document).on('click', '.sk-use-open', function (e) {
        e.preventDefault();
        $('.modal.in').modal('hide');
        $('#sk-use-modal').modal('show');
    });

    $(document).on('click', '.sk-admin-gift', function (e) {
        e.preventDefault();

        var skin = $(this).data('skin');

        $('#sk-modal').one('hidden.bs.modal', function () {
            GiftModal.open({ skin: skin });
        }).modal('hide');
    });

    $(document).on('click', '.sk-unequip', function (e) {
        e.preventDefault();
        e.stopPropagation();

        var $button = $(this);
        var weaponName = Hub.escapeHtml($button.data('weapon-name'));

        if (!confirm(texts.confirmUnequip))
            return;

        post('/api/shop/weapons/' + encodeURIComponent($button.data('weapon')) + '/unequip', null, $button, function () {
            var html = '<p>' + Hub.format(texts.unequipSuccess, weaponName) + '</p>'
                + '<p class="sk-notice">' + texts.nextMap + '</p>';

            showResult(texts.unequipSuccessTitle, html, true);
        });
    });

    $('#sk-result').on('hidden.bs.modal', function () {
        if (reloadOnClose)
            location.reload();
    });

    function applyFilters() {
        $('.tab-pane').each(function () {
            var $pane = $(this);
            var total = 0;

            $pane.find('.sk-section').each(function () {
                var visible = 0;

                $(this).find('.sk-item').each(function () {
                    var $item = $(this);
                    var show = (weapon === 'all' || $item.data('weapon') === weapon)
                        && (rarity === 'all' || $item.data('rarity') === rarity)
                        && (!onlyAffordable || $item.data('affordable') === 1);

                    $item.toggle(show);

                    if (show)
                        visible++;
                });

                $(this).toggle(visible > 0);
                total += visible;
            });

            if ($pane.find('.sk-item').length > 0)
                $pane.find('.sk-empty').toggleClass('hidden', total > 0);
        });
    }

    $('#sk-weapon-filter').on('change', function () {
        weapon = $(this).val();
        applyFilters();
    });

    $('#sk-rarity-filter').on('change', function () {
        rarity = $(this).val();
        applyFilters();
    });

    $('#sk-only-affordable').on('change', function () {
        onlyAffordable = this.checked;
        applyFilters();
    });

    $('.sk-tabs a').on('shown.bs.tab', function () {
        history.replaceState(null, '', $(this).attr('href'));
    });

    $('.sk-tabs a').filter(function () {
        return $(this).attr('href') === location.hash;
    }).tab('show');

    if (location.hash.indexOf('#skin-') === 0) {
        var $card = $('#sk-shop').find('[id="' + location.hash.substring(1) + '"]');

        if ($card.length > 0)
            openDetails($card);
    }
});
