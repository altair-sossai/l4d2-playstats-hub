var championshipsOverviewConfig = JSON.parse(document.getElementById("championships-overview-config").textContent);

$(function () {
    var $countdowns = $('[data-countdown]');

    if ($countdowns.length === 0)
        return;

    function pad(value) {
        return value < 10 ? '0' + value : String(value);
    }

    function tick() {
        var now = Date.now();

        $countdowns.each(function () {
            var $countdown = $(this);
            var left = Math.max(0, Math.floor((Date.parse($countdown.attr('data-countdown')) - now) / 1000));

            $countdown.find('[data-countdown-days]').text(pad(Math.floor(left / 86400)));
            $countdown.find('[data-countdown-hours]').text(pad(Math.floor(left % 86400 / 3600)));
            $countdown.find('[data-countdown-minutes]').text(pad(Math.floor(left % 3600 / 60)));
            $countdown.find('[data-countdown-seconds]').text(pad(left % 60));
            $countdown.toggleClass('championship-countdown-done', left === 0);
        });
    }

    tick();
    setInterval(tick, championshipsOverviewConfig.values.tickInterval);
});
