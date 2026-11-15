namespace L4D2PlayStats.Hub.Core.Modules.Patents;

public class Patent(int level, string name)
{
    public int Level { get; } = level;
    public string Name { get; } = name;

    public int Experience
    {
        get
        {
            var normalized = (Level - AppConsts.PatentMinLevel) / ((double)AppConsts.PatentMaxLevel - AppConsts.PatentMinLevel);
            var exponential = Math.Pow(normalized, 2);
            var result = AppConsts.PatentMinExperience + exponential * (AppConsts.PatentMaxExperience - AppConsts.PatentMinExperience);

            return (int)(Math.Floor(result / 10) * 10);
        }
    }

    public string Image => $"/imgs/patents/{Level:00}.png";
}

public static class PatentCatalog
{
    private static readonly List<Patent> Patents =
    [
        new(1, "Bronze #1"),
        new(2, "Bronze #2"),
        new(3, "Bronze #3"),
        new(4, "Bronze #4"),
        new(5, "Bronze #5"),
        new(6, "Silver #1"),
        new(7, "Silver #2"),
        new(8, "Silver #3"),
        new(9, "Silver #4"),
        new(10, "Silver #5"),
        new(11, "Gold #1"),
        new(12, "Gold #2"),
        new(13, "Gold #3"),
        new(14, "Gold #4"),
        new(15, "Global")
    ];

    public static IReadOnlyList<Patent> All => Patents;

    public static Patent For(decimal experience)
    {
        for (var index = Patents.Count - 1; index >= 0; index--)
            if (experience >= Patents[index].Experience)
                return Patents[index];

        return Patents[0];
    }

    public static Patent? Next(decimal experience)
    {
        var current = For(experience);

        return Patents.FirstOrDefault(p => p.Level > current.Level);
    }
}

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
