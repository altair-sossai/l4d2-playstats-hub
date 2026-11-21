var matchesDetailsConfig = JSON.parse(document.getElementById("matches-details-config").textContent);

function showMatch(result) {
    if (result && result.matchId)
        location.href = matchesDetailsConfig.values["AppConsts.MatchesUrl"] + "/" + result.matchId;
    else
        location.href = matchesDetailsConfig.values["AppConsts.MatchesUrl"];
}

function editScore(statisticId, team, score, message) {
    var output = prompt(message, score);

    if (!output)
        return;

    var score = +output;

    if (isNaN(score))
        return;

    if (score < 0 || score > 10000) {
        alert(matchesDetailsConfig.values["L[\"Invalid score range\"].Value"]);
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
        headers: matchesDetailsConfig.values["new Dictionary<string, string?> { [AppConsts.AntiforgeryHeader] = Antiforgery.GetAndStoreTokens(Context).RequestToken }"],
        data: JSON.stringify(command)
    }).done(showMatch);
}
