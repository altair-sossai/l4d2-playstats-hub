var serversIndexConfig = JSON.parse(document.getElementById("servers-index-config").textContent);

var refreshInterval = serversIndexConfig.values.serverPickerRefreshInMilliseconds;
var requestTimeout = serversIndexConfig.values.livePageRequestTimeoutInMilliseconds;
var dd = new diffDOM.DiffDOM();

function sync(current, html) {
    var source = document.createElement('div');
    source.id = current.id;
    source.innerHTML = html;

    var diffs = dd.diff(current, source);
    dd.apply(current, diffs);
}

function refreshServers() {
    $.ajax({
        url: serversIndexConfig.values.serversUrl + '?cards=true',
        type: 'GET',
        timeout: requestTimeout,
        success: function (html) {
            var target = document.getElementById('server-cards');
            sync(target, html);
        },
        complete: function () {
            setTimeout(refreshServers, refreshInterval);
        }
    });
}

$(document).ready(function () {
    setTimeout(refreshServers, refreshInterval);
});
