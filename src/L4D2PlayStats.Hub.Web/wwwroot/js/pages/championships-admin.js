var championshipsAdminConfig = JSON.parse(document.getElementById("championships-admin-config").textContent);

$(function () {
    var texts = championshipsAdminConfig.texts;
    var $game = $('#game-edit-modal');
    var $match = $('#game-match');

    function sections(result) {
        $game.find('[data-game-section]').each(function () {
            var shown = $(this).attr('data-game-section').split(' ').indexOf(result) >= 0;

            $(this).toggle(shown);
            $(this).find('input, select').prop('disabled', !shown);
        });

        $('#game-score-a, #game-score-b').prop('required', result === 'typed');
        $match.prop('required', result === 'match');
    }

    function candidates(url, current) {
        $match.empty().append($('<option>').val('').text(texts.loading));

        $.getJSON(url).done(function (items) {
            $match.empty();

            if (items.length === 0)
                $match.append($('<option>').val('').text(texts.noMatch));

            items.forEach(function (item) {
                var option = $('<option>').val(item.matchId).text(item.label + (item.linked ? ' · ' + texts.linked : ''));

                if (item.linked && String(item.matchId) !== String(current))
                    option.prop('disabled', true);

                $match.append(option);
            });

            if (current)
                $match.val(String(current));
        });
    }

    $(document).on('click', '[data-game-edit]', function () {
        var data = $(this).data();
        var result = data.gameResult || 'none';
        var teamA = $(this).attr('data-game-team-a');
        var teamB = $(this).attr('data-game-team-b');

        $('#game-form').attr('action', $(this).attr('data-game-edit'));
        $('#game-edit-title').text(Hub.format(texts.gameTitle, data.gameNumber, teamA, teamB));
        $game.find('.game-team-a').text(teamA);
        $game.find('.game-team-b').text(teamB);
        $('#game-campaign').val(data.gameCampaign === undefined || data.gameCampaign === '' ? '' : String(data.gameCampaign));
        $('#game-original-campaign').val(data.gameCampaign === undefined ? '' : data.gameCampaign);
        $('#game-score-a').val(data.gameScoreA === undefined ? '' : data.gameScoreA);
        $('#game-score-b').val(data.gameScoreB === undefined ? '' : data.gameScoreB);
        $('#game-played-at').val($(this).attr('data-game-played-at') || '');
        $('#game-penalty-a').val(data.gamePenaltyA || 0);
        $('#game-penalty-b').val(data.gamePenaltyB || 0);
        $('#game-notes').val($(this).attr('data-game-notes') || '');
        $game.find('input[name="result"][value="' + result + '"]').prop('checked', true);
        sections(result);
        candidates($(this).attr('data-game-candidates'), data.gameMatch);
        $game.modal('show');
    });

    $game.on('change', '[data-game-result-option]', function () {
        sections($(this).val());
    });

    $(document).on('click', '[data-participant-pot]', function () {
        var $modal = $('#participant-pot-modal');

        $('#participant-pot-form').attr('action', $(this).attr('data-participant-pot'));
        $modal.find('.championship-pot-name').text($(this).attr('data-participant-name'));
        $('#participant-pot').val($(this).attr('data-participant-value') || 0);
        $modal.modal('show');
    });

    $(document).on('click', '[data-participants-picker]', function (event) {
        var $target = $($(this).attr('data-participants-picker'));

        event.preventDefault();

        PlayerPicker.swap($(this).closest('.modal'), function (player) {
            var value = $.trim($target.val());

            $target.val(value ? value + '\n' + player.communityId : player.communityId);
        });
    });
});
