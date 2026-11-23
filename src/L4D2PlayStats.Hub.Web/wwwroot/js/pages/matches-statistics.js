var matchesStatisticsConfig = JSON.parse(document.getElementById("matches-statistics-config").textContent);

// The pencil next to a player: another player takes that line of the round half, with its numbers, for a substitute.
// The match and the ranking are rebuilt, and the page opens again on the same map.
document.addEventListener('DOMContentLoaded', function () {
    var texts = matchesStatisticsConfig.texts;
    var values = matchesStatisticsConfig.values;

    $(document).on('click', '.ms-replace', function () {
        var $button = $(this).tooltip('hide');
        var name = $button.attr('data-name');

        PlayerPicker.open(function (player) {
            if (!player || player.communityId === $button.attr('data-player'))
                return;

            if (!window.confirm(Hub.format(texts["Replace {0} with {1} in this round half? The match and the ranking are recalculated."], name, player.name || player.communityId)))
                return;

            $.ajax({
                url: values.replacePlayerUrl,
                type: 'POST',
                contentType: 'application/json',
                headers: values.antiforgeryHeaders,
                data: JSON.stringify({
                    half: +$button.attr('data-half'),
                    survivor: $button.attr('data-survivor') === 'true',
                    replaced: $button.attr('data-player'),
                    replacement: player.communityId,
                    replacementName: player.name || ''
                })
            }).done(function (result) {
                location.href = result && result.matchId ? values.matchesUrl + '/' + result.matchId + '/statistics/' + result.fileId : values.matchesUrl;
            }).fail(function (xhr) {
                window.alert(Hub.errorText(xhr, texts["The player could not be replaced. Try again."]));
            });
        }, { hint: Hub.format(texts["Choose the player who takes the place of {0} in this round half, with the numbers of this line."], name) });
    });
});
