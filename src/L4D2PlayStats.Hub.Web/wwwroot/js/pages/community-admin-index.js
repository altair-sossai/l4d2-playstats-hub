$('.ca-tabs a[data-toggle="tab"]').on('shown.bs.tab', function () {
    try {
        var params = new URLSearchParams(window.location.search);
        params.set('tab', $(this).attr('data-tab'));
        history.replaceState(null, '', window.location.pathname + '?' + params.toString());
    } catch (e) {
    }
});
