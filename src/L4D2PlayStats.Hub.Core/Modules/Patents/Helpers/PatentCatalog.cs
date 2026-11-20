namespace L4D2PlayStats.Hub.Core.Modules.Patents.Helpers;

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