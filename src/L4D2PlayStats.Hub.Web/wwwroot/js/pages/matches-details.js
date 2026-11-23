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
