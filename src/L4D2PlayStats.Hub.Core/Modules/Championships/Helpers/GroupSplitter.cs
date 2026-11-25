namespace L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;

public static class GroupSplitter
{
    public static List<List<int>> Split(IReadOnlyList<int> seededTeams, int groups)
    {
        var count = Math.Clamp(groups, 1, Math.Max(1, seededTeams.Count / 2));
        var result = Enumerable.Range(0, count).Select(_ => new List<int>()).ToList();

        for (var index = 0; index < seededTeams.Count; index++)
        {
            var row = index / count;
            var column = index % count;
            var group = row % 2 == 0 ? column : count - 1 - column;

            result[group].Add(seededTeams[index]);
        }

        return result;
    }

    public static string Name(int index)
    {
        return index < AppConsts.ChampionshipGroupNames.Length ? AppConsts.ChampionshipGroupNames[index].ToString() : (index + 1).ToString();
    }
}
