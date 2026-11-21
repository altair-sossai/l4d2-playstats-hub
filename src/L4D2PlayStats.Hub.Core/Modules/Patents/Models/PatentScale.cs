namespace L4D2PlayStats.Hub.Core.Modules.Patents.Models;

public class PatentScale
{
    private readonly List<Patent> _patents;

    public PatentScale(int maxExperience, double curve, IReadOnlyList<string> names)
    {
        MaxExperience = maxExperience;
        Curve = curve;
        _patents = [.. names.Select((name, index) => new Patent(index + 1, name, Experience(index + 1, names.Count, maxExperience, curve)))];
    }

    public int MaxExperience { get; }

    public double Curve { get; }

    public IReadOnlyList<Patent> All => _patents;

    public Patent For(decimal experience)
    {
        for (var index = _patents.Count - 1; index >= 0; index--)
            if (experience >= _patents[index].Experience)
                return _patents[index];

        return _patents[0];
    }

    public Patent? Next(decimal experience)
    {
        var current = For(experience);

        return _patents.FirstOrDefault(p => p.Level > current.Level);
    }

    public static int Experience(int level, int levels, int maxExperience, double curve)
    {
        var normalized = (level - AppConsts.PatentMinLevel) / ((double)levels - AppConsts.PatentMinLevel);
        var exponential = Math.Pow(normalized, curve);
        var result = AppConsts.PatentMinExperience + exponential * (maxExperience - AppConsts.PatentMinExperience);

        return (int)(Math.Floor(result / AppConsts.PatentExperienceRounding) * AppConsts.PatentExperienceRounding);
    }
}
