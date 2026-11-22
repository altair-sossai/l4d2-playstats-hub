var communityRankingModalsConfig = JSON.parse(document.getElementById("community-ranking-modals-config").textContent);

(function () {
    var positionText = communityRankingModalsConfig.texts["#{0} in the community ranking"];

    function percent(value, total) {
        return total > 0 ? (value * 100 / total).toFixed(2) + '%' : '0%';
    }

    function share(value, total) {
        return (total > 0 ? Math.round(value * 100 / total) : 0) + '%';
    }

    document.addEventListener('click', function (event) {
        var row = event.target.closest('#table-community-ranking tbody tr[data-player]');

        if (!row || event.target.closest('a'))
            return;

        var data = row.dataset;
        var modal = $('#community-player-modal');
        var wins = parseInt(data.wins, 10);
        var draws = parseInt(data.draws, 10);
        var losses = parseInt(data.losses, 10);
        var total = wins + draws + losses;

        modal.find('.crm-avatar').attr('src', '/avatar/' + data.player).attr('alt', data.name);
        modal.find('#community-player-name').text(data.name);
        modal.find('.crm-position').text(positionText.replace('{0}', data.position));
        modal.find('.crm-patent').text(data.patent || '');
        modal.find('.crm-rating').text(data.rating);
        modal.find('.crm-comparisons').text(data.comparisons);
        modal.find('.crm-wins').text(wins + ' (' + share(wins, total) + ')');
        modal.find('.crm-draws').text(draws + ' (' + share(draws, total) + ')');
        modal.find('.crm-losses').text(losses + ' (' + share(losses, total) + ')');
        modal.find('.crm-bar-win').css('width', percent(wins, total));
        modal.find('.crm-bar-draw').css('width', percent(draws, total));
        modal.find('.crm-bar-loss').css('width', percent(losses, total));
        modal.find('.crm-votes').attr('href', communityRankingModalsConfig.values.communityVotesPage + '?player=' + data.player);
        modal.find('.crm-profile').toggle(!!data.profile).attr('href', data.profile || '#');
        modal.modal('show');
    });
})();
