var layout3Config = JSON.parse(document.getElementById("layout-3-config").textContent);

var titlePlayersRegex = /^🟢\s*\d+\/\d+\s*\|\s*/;
var liveServerEnabled = layout3Config.values.liveServer;
var robotVisitor = new RegExp(layout3Config.values.robotUserAgentPattern, 'i').test(navigator.userAgent);
var skinShopEnabled = layout3Config.values.skinShop;

function getBaseTitle() {
    return document.title.replace(titlePlayersRegex, '');
}

function pulsate(element, options) {
    if (!options)
        options = {};

    if (!options.start)
        options.start = {};

    if (!options.start.opacity)
        options.start.opacity = 0.6;

    if (!options.start.interval)
        options.start.interval = 700;

    if (!options.end)
        options.end = {};

    if (!options.end.opacity)
        options.end.opacity = 1;

    if (!options.end.interval)
        options.end.interval = 700;

    $(element).animate(options.start, options.start.interval, function () {
        $(this).animate(options.end, options.end.interval, function () {
            pulsate(this, options);
        });
    });
}

function updateServerInfo() {
    if (!liveServerEnabled || robotVisitor)
        return;

    $.ajax({
        url: '/api/server',
        method: 'GET',
        success: function (serverInfo) {
            var baseTitle = getBaseTitle();

            if (serverInfo && serverInfo.address) {
                $('#connect-icon img').attr('alt', 'connect ' + serverInfo.address).attr('title', 'connect ' + serverInfo.address).attr('data-original-title', 'connect ' + serverInfo.address);
                $('#connect-icon').show();
            } else {
                $('#connect-icon').hide();
            }

            if (!serverInfo || serverInfo.players == 0) {
                $('#server-info').hide();
                $('#game-info-chat').hide();
                document.title = baseTitle;
                fitNavbar();
                return;
            }

            $('#server-status').text('● ' + serverInfo.players + '/' + serverInfo.max_players);
            $('#server-info').show();
            $('#game-info-chat').show();
            fitNavbar();
            document.title = '🟢 ' + serverInfo.players + '/' + serverInfo.max_players + ' | ' + baseTitle;
        },
        complete: function () {
            setTimeout(nextServerInfo, 4000);
        }
    });
}

var serverInfoStarted = false;
var serverInfoPaused = false;
var serverInfoStartEvents = ['pointermove', 'pointerdown', 'scroll', 'keydown', 'touchstart'];

function nextServerInfo() {
    if (document.hidden) {
        serverInfoPaused = true;
        return;
    }

    updateServerInfo();
}

function startServerInfo() {
    if (serverInfoStarted)
        return;

    serverInfoStarted = true;

    serverInfoStartEvents.forEach(function (name) {
        window.removeEventListener(name, startServerInfo);
    });

    updateServerInfo();
}

function startServerInfoOnInteraction() {
    serverInfoStartEvents.forEach(function (name) {
        window.addEventListener(name, startServerInfo, { passive: true });
    });

    document.addEventListener('visibilitychange', function () {
        if (document.hidden || !serverInfoPaused)
            return;

        serverInfoPaused = false;
        updateServerInfo();
    });
}

function createChart(canvas) {
    var ctx = canvas.getContext('2d');
    var chartData = $(canvas).data('chart');

    if (chartData.options && chartData.options.tankWipe)
        addTankWipeTooltip(chartData.options);

    new Chart(ctx, chartData);
}

function addTankWipeTooltip(options) {
    var label = options.tankWipe.label;

    options.tooltips = options.tooltips || {};
    options.tooltips.callbacks = options.tooltips.callbacks || {};
    options.tooltips.callbacks.afterLabel = function (item, data) {
        var wipes = data.datasets[item.datasetIndex].tankWipes || [];
        var name = wipes[item.index];

        return name ? label + ': ' + name : '';
    };
}

var tankWipeImages = {};

function tankWipeImage(chart, url) {
    var image = tankWipeImages[url];

    if (!image) {
        image = new Image();
        image.src = url;
        tankWipeImages[url] = image;
    }

    if (!image.complete)
        $(image).one('load', function () { chart.draw(); });

    return image;
}

if (window.Chart) Chart.plugins.register({
    afterDatasetsDraw: function (chart) {
        var tankWipe = chart.options.tankWipe;
        if (!tankWipe)
            return;

        var image = tankWipeImage(chart, tankWipe.image);
        if (!image.complete || !image.naturalWidth)
            return;

        var ctx = chart.ctx;
        var size = 16;

        chart.data.datasets.forEach(function (dataset, datasetIndex) {
            var meta = chart.getDatasetMeta(datasetIndex);
            if (!dataset.tankWipes || meta.hidden)
                return;

            meta.data.forEach(function (bar, index) {
                if (!dataset.tankWipes[index])
                    return;

                var model = bar._model;
                var top = Math.min(model.y, model.base);
                var x = model.x - size / 2;
                var y = top - size - 3;

                if (y < chart.chartArea.top)
                    y = top + 3;

                ctx.save();
                ctx.beginPath();
                ctx.arc(x + size / 2, y + size / 2, size / 2 + 1.5, 0, Math.PI * 2);
                ctx.fillStyle = '#d9534f';
                ctx.fill();
                ctx.beginPath();
                ctx.arc(x + size / 2, y + size / 2, size / 2, 0, Math.PI * 2);
                ctx.clip();
                ctx.drawImage(image, x, y, size, size);
                ctx.restore();
            });
        });
    }
});

function isElementInViewport(el, margin) {
    margin = margin || 150;
    var rect = el.getBoundingClientRect();

    return (
        rect.top >= -margin &&
        rect.left >= -margin &&
        rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) + margin &&
        rect.right <= (window.innerWidth || document.documentElement.clientWidth) + margin
    );
}

function lazyLoadCharts() {
    $('.app-chart').each(function () {
        if ($(this).data('loaded'))
            return;

        if (!isElementInViewport(this))
            return;

        createChart(this);

        $(this).data('loaded', true);
    });
}

function isObject(value) {
    return value !== null && typeof value === 'object' && !Array.isArray(value);
}

var navStarsKey = 'nav-stars-' + layout3Config.values.communityId;

function setNavStars(balance) {
    var $stars = $('.nav-stars');

    $stars.find('.nav-stars-value').text(balance.toLocaleString(document.documentElement.lang || undefined));
    $stars.find('.nav-stars-label').text(balance === 1 ? layout3Config.texts["star available"] : layout3Config.texts["stars available"]);
    $stars.show();
    fitNavbar();

    try {
        sessionStorage.setItem(navStarsKey, balance);
    } catch (e) {
    }
}

function loadNavStars() {
    if (!skinShopEnabled || $('.nav-stars').length === 0)
        return;

    try {
        var cached = sessionStorage.getItem(navStarsKey);

        if (cached !== null)
            setNavStars(parseInt(cached, 10));
    } catch (e) {
    }

    $.ajax({
        url: '/api/shop/wallet',
        method: 'GET',
        success: function (wallet) {
            setNavStars(wallet.balance);
        }
    });
}

$(window).on('scroll resize', function () {
    lazyLoadCharts();
});

function setUpFieldHelp() {
    var more = $('#field-help-modal').data('more');

    $('.field-help-more').each(function () {
        var link = $(this);

        link.attr('title', link.data('help-summary') + ' ' + more);
    });

    $(document).on('click', '.field-help-more', function (event) {
        var link = $(this);
        var body = $('#field-help-modal .modal-body').empty();

        event.preventDefault();
        link.tooltip('hide');

        $('#field-help-modal .modal-title').empty().append('<span class="glyphicon glyphicon-question-sign"></span> ').append(document.createTextNode(link.data('help-title')));
        $('<div class="field-help-summary">').text(link.data('help-summary')).appendTo(body);

        String(link.data('help-details')).split('\n').forEach(function (paragraph) {
            var text = paragraph.trim();
            var term = /^([^:.]{2,40}):\s+(.+)$/.exec(text);
            var item = $('<p>');

            if (!text)
                return;

            if (term)
                item.append($('<strong>').text(term[1])).append(document.createTextNode(' ' + term[2]));
            else
                item.text(text);

            item.appendTo(body);
        });

        $('#field-help-modal').modal('show');
    });

    $(document).on('click', 'label .field-help', function (event) {
        event.preventDefault();
    });
}

$(document).ready(function () {
    setUpFieldHelp();

    $('[data-toggle="tooltip"]').tooltip();

    $('.pulsate').each(function () {
        pulsate(this);
    });

    startServerInfoOnInteraction();

    loadNavStars();

    lazyLoadCharts();
});
