using L4D2PlayStats.Hub.Core.Modules.Patents.Helpers;

namespace L4D2PlayStats.Hub.Core.Modules.Patents.Models;

public class PatentProgress
{
    private readonly decimal _experience;
    private readonly decimal? _experienceDifference;

    public PatentProgress(decimal experience, decimal? previousExperience)
    {
        _experience = experience;
        _experienceDifference = previousExperience == null ? null : experience - previousExperience;

        Patent = PatentCatalog.For(experience);
        NextPatent = PatentCatalog.Next(experience);

        if (previousExperience != null)
            PreviousPatent = PatentCatalog.For(previousExperience.Value);
    }

    public Patent Patent { get; }
    public Patent? NextPatent { get; }
    public Patent? PreviousPatent { get; }
    public bool MaxLevel => NextPatent == null;
    public bool LevelUp => PreviousPatent != null && Patent.Level > PreviousPatent.Level;
    public bool LevelDrop => PreviousPatent != null && Patent.Level < PreviousPatent.Level;

    public decimal Progress
    {
        get
        {
            if (MaxLevel)
                return 1;

            var current = _experience - Patent.Experience;
            var required = NextPatent!.Experience - Patent.Experience;

            return current / required;
        }
    }

    public decimal ProgressDifference
    {
        get
        {
            if (MaxLevel || LevelUp || LevelDrop || _experienceDifference == null)
                return 0;

            var current = Math.Abs(_experienceDifference.Value);
            var required = NextPatent!.Experience - Patent.Experience;

            return current / required;
        }
    }
}
