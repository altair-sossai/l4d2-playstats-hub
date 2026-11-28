(function (window, $) {
    var Hub = window.Hub || {};

    Hub.escapeHtml = function (text) {
        return $('<div/>').text(text == null ? '' : text).html().replace(/"/g, '&quot;').replace(/'/g, '&#39;');
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

    Hub.confirm = function (options) {
        var settings = typeof options === 'string' ? { text: options } : (options || {});
        var $modal = $('#confirm-modal');
        var deferred = $.Deferred();
        var accepted = false;

        if ($modal.length === 0)
            return deferred.resolve(window.confirm(settings.text)).promise();

        var $ok = $modal.find('[data-confirm-ok]');

        $modal.find('.modal-title').text(settings.title || $modal.attr('data-title'));
        $modal.find('.confirm-modal-text').text(settings.text || '');
        $ok.text(settings.ok || $modal.attr('data-ok')).toggleClass('btn-danger', !!settings.danger).toggleClass('btn-primary', !settings.danger);
        $ok.off('click.hubConfirm').on('click.hubConfirm', function () {
            accepted = true;
            $modal.modal('hide');
        });
        $modal.off('shown.bs.modal.hubConfirm').one('shown.bs.modal.hubConfirm', function () {
            $('.modal-backdrop').last().addClass('confirm-modal-backdrop');
            $ok.trigger('focus');
        });
        $modal.off('hidden.bs.modal.hubConfirm').one('hidden.bs.modal.hubConfirm', function () {
            if ($('.modal.in').length > 0)
                $('body').addClass('modal-open');

            deferred.resolve(accepted);
        });
        $modal.modal('show');

        return deferred.promise();
    };

    function confirmSettings($element) {
        return {
            text: $element.attr('data-confirm'),
            title: $element.attr('data-confirm-title'),
            ok: $element.attr('data-confirm-ok'),
            danger: $element.is('[data-confirm-danger]')
        };
    }

    $(document).on('submit', 'form[data-confirm]', function (event) {
        var form = this;
        var submitter = event.originalEvent && event.originalEvent.submitter;

        if (form.getAttribute('data-confirmed') === 'true' || $(form).data('related-answered')) {
            form.removeAttribute('data-confirmed');
            return;
        }

        event.preventDefault();
        event.stopImmediatePropagation();

        Hub.confirm(confirmSettings($(form))).done(function (accepted) {
            if (!accepted)
                return;

            form.setAttribute('data-confirmed', 'true');

            if (form.requestSubmit)
                form.requestSubmit(submitter && submitter.form === form ? submitter : undefined);
            else
                form.submit();
        });
    });

    document.addEventListener('click', function (event) {
        var element = event.target && event.target.closest ? event.target.closest('button[data-confirm], a[data-confirm], input[data-confirm]') : null;

        if (!element || element.disabled)
            return;

        if (element.getAttribute('data-confirmed') === 'true') {
            element.removeAttribute('data-confirmed');
            return;
        }

        event.preventDefault();
        event.stopImmediatePropagation();

        Hub.confirm(confirmSettings($(element))).done(function (accepted) {
            if (!accepted)
                return;

            if (element.tagName === 'A') {
                window.location.href = element.href;
                return;
            }

            var form = element.form;

            element.setAttribute('data-confirmed', 'true');

            if (form)
                form.setAttribute('data-confirmed', 'true');

            element.click();
        });
    }, true);

    window.Hub = Hub;
})(window, jQuery);
