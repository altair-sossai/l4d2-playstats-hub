var playersDetailsConfig = JSON.parse(document.getElementById("players-details-config").textContent);

function reloadPage() {
    location.reload();
}

var punishmentSeasonId = playersDetailsConfig.values.seasonId;
var punishmentHeaders = playersDetailsConfig.values.antiforgeryHeaders;

function addPunishments(communityId, lostExperiencePoints) {
    var command = { 
        communityId: communityId, 
        lostExperiencePoints: lostExperiencePoints,
        seasonId: punishmentSeasonId
    };

    $.ajax({
        url: "/api/punishments",
        type: "POST",
        contentType: "application/json",
        headers: punishmentHeaders,
        data: JSON.stringify(command)
    }).done(reloadPage);
}

function deletePunishments(communityId) {
    $.ajax({
        url: "/api/punishments/" + communityId + "?" + playersDetailsConfig.values.playerSeasonParameter + "=" + punishmentSeasonId,
        type: "DELETE",
        headers: punishmentHeaders
    }).done(reloadPage);
}

function editPunishment(communityId, punishment) {
    var output = prompt(playersDetailsConfig.texts["How many experience points should the user lose?"], punishment);

    if (!output)
        return;

    var lostExperiencePoints = +output;

    if (isNaN(lostExperiencePoints))
        return;

    else if (lostExperiencePoints > 0)
        Hub.confirm(Hub.format(playersDetailsConfig.texts["Take {0} experience points from this player in this season? The ranking is calculated again."], lostExperiencePoints)).done(function (accepted) {
            if (accepted)
                addPunishments(communityId, lostExperiencePoints);
        });

    else if (lostExperiencePoints === 0)
        Hub.confirm(playersDetailsConfig.texts["Remove the punishments of this player in this season?"]).done(function (accepted) {
            if (accepted)
                deletePunishments(communityId);
        });
}

$(function () {
    var $tabs = $('#player-tabs a[data-toggle="tab"]');

    $tabs.on('shown.bs.tab', function () {
        history.replaceState(null, '', $(this).attr('href'));
        $(window).trigger('resize');
    });

    if (location.hash)
        $tabs.filter('[href="' + location.hash + '"]').tab('show');

    $('.players-dropdown-menu a, .relation-card-item .panel-heading a, .period-selector .dropdown-menu a').on('click', function () {
        if (location.hash)
            this.href = this.href.split('#')[0] + location.hash;
    });

    $('#btn-show-more-relations').click(function () {
        var $button = $(this);
        var $cards = $('.relation-card-item');

        if ($cards.hasClass('hidden')) {
            $cards.removeClass('hidden');
            $button.text($button.data('less'));
        } else {
            $cards.each(function (index) {
                if (index >= $button.data('visible')) {
                    $(this).addClass('hidden');
                }
            });
            $button.text($button.data('more'));
        }
    });
});
