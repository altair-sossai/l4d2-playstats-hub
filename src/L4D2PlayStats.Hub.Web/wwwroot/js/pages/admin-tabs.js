var adminTabsConfig = JSON.parse(document.getElementById("admin-tabs-config").textContent);

document.addEventListener('DOMContentLoaded', function () {
    function current(key) {
        document.querySelectorAll('input[name="' + adminTabsConfig.values["AppConsts.AdminTabQuery"] + '"][data-admin-tab-field]').forEach(function (field) {
            field.value = key;
        });
    }

    document.addEventListener('invalid', function (event) {
        var pane = event.target.closest('.tab-pane');

        if (!pane || pane.classList.contains('active'))
            return;

        $('.admin-tabs a[href="#' + pane.id + '"]').tab('show');
        setTimeout(function () {
            event.target.reportValidity();
        }, 200);
    }, true);

    $('.admin-tabs a[data-toggle="tab"]').on('shown.bs.tab', function () {
        var key = $(this).attr('data-admin-tab');

        current(key);

        try {
            var params = new URLSearchParams(window.location.search);
            params.set(adminTabsConfig.values["AppConsts.AdminTabQuery"], key);
            history.replaceState(null, '', window.location.pathname + '?' + params.toString() + window.location.hash.replace(new RegExp('^#' + adminTabsConfig.values["AppConsts.AdminTabPanePrefix"] + '.*$'), ''));
        } catch (e) {
        }
    });
});
