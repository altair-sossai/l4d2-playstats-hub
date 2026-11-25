namespace L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;

public static class ChampionshipShuffle
{
    public static List<T> Shuffle<T>(IEnumerable<T> items, Func<int, int> next)
    {
        var list = items.ToList();

        for (var index = list.Count - 1; index > 0; index--)
        {
            var other = next(index + 1);

            (list[index], list[other]) = (list[other], list[index]);
        }

        return list;
    }
}
