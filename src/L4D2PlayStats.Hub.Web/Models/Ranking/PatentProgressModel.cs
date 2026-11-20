using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Patents;
using L4D2PlayStats.Hub.Core.Modules.Patents.Models;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;
using L4D2PlayStats.Hub.Web.Infrastructure.Localization;
using Microsoft.Extensions.Localization;

namespace L4D2PlayStats.Hub.Web.Models.Ranking;

public class PatentProgressModel(IStringLocalizer<SharedResource> localizer, RankingPlayer player)
{
    private readonly PatentProgress _progress = new(player.Experience, player.PreviousExperience);

    public Patent Patent => _progress.Patent;
    public Patent? NextPatent => _progress.NextPatent;
    public bool MaxLevel => _progress.MaxLevel;
    public bool LevelUp => _progress.LevelUp;
    public bool LevelDrop => _progress.LevelDrop;
    public decimal Progress => _progress.Progress;
    public decimal ProgressDifference => _progress.ProgressDifference;
    public decimal? ExperienceDifference => player.ExperienceDifference;

    public string ProgressBarType => MaxLevel || LevelUp ? "success"
        : LevelDrop ? "danger"
        : "info";

    public bool ProgressBarPulsate => !MaxLevel && ProgressBarType != "info";

    public string ProgressBarWidth
    {
        get
        {
            var progress = Progress;

            if (ProgressDifference != 0 && player.ExperienceDifference is > 0)
                progress -= ProgressDifference;

            return $"{Math.Round(progress * 100, 2).ToString(CultureInfo.InvariantCulture)}%";
        }
    }

    public string PreviousExperienceProgressBarType => ExperienceDifference == null ? "info" : ExperienceDifference.Value > 0 ? "success" : "danger";

    public string PreviousExperienceProgressBarWidth => $"{Math.Round(ProgressDifference * 100, 2).ToString(CultureInfo.InvariantCulture)}%";

    public string Tooltip => string.Join("<br/>", TooltipMessages());

    private IEnumerable<string> TooltipMessages()
    {
        if (LevelUp)
            yield return localizer["Level Up!"];

        if (LevelDrop)
            yield return localizer["Level Drop!"];

        if (MaxLevel)
        {
            yield return localizer["Max patent reached"];
            yield break;
        }

        if (ExperienceDifference is > 0)
            yield return localizer["Last game: +{0:N0} XP", ExperienceDifference.Value];

        if (ExperienceDifference is < 0)
            yield return localizer["Last game: {0:N0} XP", ExperienceDifference.Value];

        if (NextPatent != null)
        {
            yield return localizer["Next Level: {0:N0}", NextPatent.Level];
            yield return localizer["XP: {0:N0} / {1:N0}", player.Experience, NextPatent.Experience];
            yield return localizer["Progress: {0:P0}", Progress];
        }
    }
}
