var serversIndexConfig = JSON.parse(document.getElementById("servers-index-config").textContent);

var refreshInterval = serversIndexConfig.values["AppConsts.ServerPickerRefreshInMilliseconds"];
var requestTimeout = serversIndexConfig.values["AppConsts.LivePageRequestTimeoutInMilliseconds"];
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
        url: serversIndexConfig.values["AppConsts.ServersUrl"] + '?cards=true',
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
