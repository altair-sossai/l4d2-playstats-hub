(function (window, $) {
    var Hub = window.Hub || {};

    Hub.escapeHtml = function (text) {
        return $('<div/>').text(text == null ? '' : text).html();
    };

    Hub.format = function (template) {
        var values = Array.prototype.slice.call(arguments, 1);

        return String(template).replace(/\{(\d+)\}/g, function (match, index) {
            return values[index];
        });
    };

    Hub.alert = function ($target, success, text) {
        $target.html('<div class="alert ' + (success ? 'alert-success' : 'alert-danger') + '">' + Hub.escapeHtml(text) + '</div>');
    };

    Hub.errorText = function (xhr, fallback) {
        return (xhr && xhr.responseJSON && xhr.responseJSON.errorMessage) || fallback;
    };

    $(document).on('submit', 'form[data-confirm]', function (event) {
        if (!window.confirm($(this).attr('data-confirm')))
            event.preventDefault();
    });

    $(document).on('click', 'button[data-confirm], a[data-confirm], input[data-confirm]', function (event) {
        if (!window.confirm($(this).attr('data-confirm'))) {
            event.preventDefault();
            event.stopImmediatePropagation();
        }
    });

    window.Hub = Hub;
})(window, jQuery);
