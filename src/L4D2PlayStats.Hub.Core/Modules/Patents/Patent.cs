namespace L4D2PlayStats.Hub.Core.Modules.Patents;

public class Patent(int level, string name)
{
    public int Level { get; } = level;
    public string Name { get; } = name;
    public string FullName => $"Level {Level:00} - {Name}";

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