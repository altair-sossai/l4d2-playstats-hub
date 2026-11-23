var matchesDetailsConfig = JSON.parse(document.getElementById("matches-details-config").textContent);

function showMatch(result) {
    if (result && result.matchId)
        location.href = matchesDetailsConfig.values.matchesUrl + "/" + result.matchId;
    else
        location.href = matchesDetailsConfig.values.matchesUrl;
}

function editScore(statisticId, team, score, message) {
    var output = prompt(message, score);

    if (!output)
        return;

    var score = +output;

    if (isNaN(score))
        return;

    if (score < 0 || score > 10000) {
        alert(matchesDetailsConfig.values.invalidScoreRange);
        return;
    }

    var command = {
        team: team,
        score: score
    };

    $.ajax({
        url: "/api/statistics/" + statisticId + "/update-score",
        type: "PUT",
        contentType: "application/json",
        headers: matchesDetailsConfig.values.antiforgeryHeaders,
        data: JSON.stringify(command)
    }).done(showMatch);
}

// "Replace a player": a substitute takes the place and the numbers of the player who left in the checked maps.
document.addEventListener('DOMContentLoaded', function () {
    var values = matchesDetailsConfig.values;
    var texts = matchesDetailsConfig.texts;
    var $modal = $('#mrp-modal');
    var $replaced = $('#mrp-replaced');
    var $replacement = $('#mrp-replacement');
    var $replacementName = $('#mrp-replacement-name');
    var $maps = $modal.find('.mrp-maps');
    var $message = $modal.find('.mrp-message');
    var $save = $modal.find('.mrp-save');

    if ($modal.length === 0)
        return;

    $.each(values.players, function (index, player) {
        $replaced.append($('<option></option>').val(player.communityId).text(player.name));
    });

    function renderMaps() {
        var replaced = $replaced.val();
        var replacement = $.trim($replacement.val());

        $maps.empty();

        $.each(values.maps, function (index, map) {
            var played = map.players.indexOf(replaced) >= 0;
            var taken = replacement !== '' && map.players.indexOf(replacement) >= 0;
            var $checkbox = $('<input type="checkbox"/>').val(map.fileId).prop('checked', played && !taken).prop('disabled', !played || taken);
            var $label = $('<label></label>').append($checkbox).append(' ').append($('<span></span>').text('#' + map.number + ' ' + map.mapName));

            if (!played || taken)
                $label.append(' ').append($('<small class="text-muted"></small>').text('(' + (taken ? texts["the player who came in already played this map"] : texts["did not play this map"]) + ')'));

            $maps.append($('<div class="checkbox"></div>').append($label));
        });
    }

    function open(replaced) {
        $replaced.val(replaced && values.players.some(function (player) { return player.communityId === replaced; }) ? replaced : $replaced.find('option:first').val());
        $replacement.val('');
        $replacementName.val('');
        $modal.find('.mrp-picked').empty();
        $message.empty();
        $save.prop('disabled', false);
        renderMaps();
        $modal.modal('show');
    }

    $(document).on('click', '.mrp-open', function () {
        $(this).tooltip('hide');
        open($(this).attr('data-replaced'));
    });

    $replaced.on('change', renderMaps);

    $replacement.on('input', function () {
        $replacementName.val('');
        $modal.find('.mrp-picked').empty();
        renderMaps();
    });

    $modal.find('.mrp-pick-player').on('click', function () {
        PlayerPicker.swap($modal, function (player) {
            $replacement.val(player.communityId);
            $replacementName.val(player.name || '');
            $modal.find('.mrp-picked').html('<img src="' + Hub.escapeHtml(player.avatarUrl) + '" alt="" style="width: 24px; border-radius: 50%; margin-right: 6px" onerror="imgOnError(this)"/>' + Hub.escapeHtml(player.name || player.communityId));
            renderMaps();
        });
    });

    $save.on('click', function () {
        var replaced = $replaced.val();
        var replacement = $.trim($replacement.val());
        var fileIds = $maps.find('input:checked').map(function () { return +this.value; }).get();

        if (!replaced) {
            Hub.alert($message, false, texts["Choose the player who left."]);
            return;
        }

        if (replacement === '') {
            Hub.alert($message, false, texts["Choose the player who came in."]);
            return;
        }

        if (fileIds.length === 0) {
            Hub.alert($message, false, texts["Choose at least one map."]);
            return;
        }

        $save.prop('disabled', true);

        $.ajax({
            url: values.replacePlayerUrl,
            type: 'POST',
            contentType: 'application/json',
            headers: values.antiforgeryHeaders,
            data: JSON.stringify({ replaced: replaced, replacement: replacement, replacementName: $replacementName.val(), fileIds: fileIds })
        }).done(function (result) {
            Hub.alert($message, true, texts["The player was replaced. Opening the match again..."]);
            setTimeout(function () { showMatch(result); }, 900);
        }).fail(function (xhr) {
            Hub.alert($message, false, Hub.errorText(xhr, texts["The player could not be replaced. Try again."]));
            $save.prop('disabled', false);
        });
    });
});
