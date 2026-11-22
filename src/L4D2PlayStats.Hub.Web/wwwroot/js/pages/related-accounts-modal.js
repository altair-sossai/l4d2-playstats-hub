var relatedAccountsModalConfig = JSON.parse(document.getElementById("related-accounts-modal-config").textContent);

document.addEventListener('DOMContentLoaded', function () {
    var texts = relatedAccountsModalConfig.texts;
    var url = relatedAccountsModalConfig.values.relatedAccountsUrl;
    var antiCheatPrefix = relatedAccountsModalConfig.values.antiCheatPrefix;
    var $modal = $('#ra-modal');
    var $list = $modal.find('.ra-list');
    var accounts = [];
    var answer = null;
    var callback = null;

    function text(group, list) {
        var key = list.indexOf(antiCheatPrefix) === 0 ? antiCheatPrefix : list;

        return texts[group][key] || texts[group][antiCheatPrefix];
    }

    function chosen() {
        return $.grep(accounts, function (account) {
            return !account.linked && $list.find('input[data-community-id="' + account.communityId + '"]').is(':checked');
        });
    }

    function refresh() {
        var count = chosen().length;

        $modal.find('.ra-all-text').text(Hub.format(texts.all, count));
        $modal.find('.ra-all').prop('disabled', count === 0);
    }

    function item(account) {
        var $item = $('<label class="list-group-item"></label>');
        var $check = $('<input type="checkbox"/>').attr('data-community-id', account.communityId);
        var meta = account.steamId + ' · ' + texts.lastSeen + ': ' + account.lastSeen + ' · ' + (account.connections === 1 ? texts.connection : Hub.format(texts.connections, account.connections));

        $check.prop('checked', !account.linked).prop('disabled', account.linked).on('change', refresh);
        $item.toggleClass('ra-linked', account.linked).append($check);
        $item.append($('<img alt="" onerror="imgOnError(this)"/>').attr('src', account.avatarUrl));
        $item.append($('<div class="ra-info"></div>').append($('<strong></strong>').text(account.name || texts.unknown)).append($('<small></small>').text(meta)));

        if (account.linked)
            $item.append($('<span class="label label-default"></span>').text(text('linked', $modal.data('list'))));

        return $item;
    }

    function show(list, found) {
        accounts = found;
        answer = null;
        $modal.data('list', list);
        $modal.find('.ra-intro').text(found.length === 1 ? texts.introOne : texts.intro);
        $modal.find('.ra-question').text(text('questions', list));
        $list.empty();

        $.each(found, function (index, account) {
            $list.append(item(account));
        });

        refresh();
        $modal.modal('show');
    }

    $modal.find('.ra-only').on('click', function () {
        answer = [];
        $modal.modal('hide');
    });

    $modal.find('.ra-all').on('click', function () {
        answer = chosen();
        $modal.modal('hide');
    });

    $modal.on('hidden.bs.modal', function () {
        var done = callback;
        var result = answer;

        callback = null;
        answer = null;

        if (done)
            done(result);
    });

    window.RelatedAccounts = {
        ask: function ($parent, player, list, done) {
            var id = $.trim(player || '');

            if (id === '') {
                done([]);
                return;
            }

            $.getJSON(url, { id: id, list: list }).done(function (result) {
                var found = (result && result.accounts) || [];
                var open = $.grep(found, function (account) { return !account.linked; });

                if (open.length === 0) {
                    done([]);
                    return;
                }

                function back(related) {
                    if ($parent.length > 0)
                        $parent.one('shown.bs.modal', function () { done(related); }).modal('show');
                    else
                        done(related);
                }

                function present() {
                    callback = back;
                    show(list, found);
                }

                if ($parent.length > 0)
                    $parent.one('hidden.bs.modal', present).modal('hide');
                else
                    present();
            }).fail(function () {
                done([]);
            });
        }
    };

    $(document).on('submit', 'form[data-related-accounts]', function (event) {
        var form = this;
        var $form = $(form);
        var $player = $($form.attr('data-related-accounts-player'));

        if ($form.data('related-answered')) {
            $form.data('related-answered', false);
            return;
        }

        $form.find('input[name="relatedIds"]').remove();

        if ($player.length === 0 || $player.prop('readonly'))
            return;

        event.preventDefault();

        if ($form.data('related-busy'))
            return;

        var submitter = event.originalEvent && event.originalEvent.submitter;

        $form.data('related-busy', true);

        window.RelatedAccounts.ask($form.closest('.modal'), $player.val(), $form.attr('data-related-accounts'), function (related) {
            $form.data('related-busy', false);

            if (related === null)
                return;

            $.each(related, function (index, account) {
                $('<input type="hidden" name="relatedIds"/>').val(account.communityId).appendTo($form);
            });

            $form.data('related-answered', true);

            if (form.requestSubmit) {
                form.requestSubmit(submitter || undefined);
                return;
            }

            if (submitter && submitter.name)
                $('<input type="hidden"/>').attr('name', submitter.name).val(submitter.value).appendTo($form);

            form.submit();
        });
    });
});
