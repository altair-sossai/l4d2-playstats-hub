namespace L4D2PlayStats.Hub.Core.Modules.Campaigns.Models;

public record CampaignDefinition(string Name, IReadOnlyList<string> Maps)
{
    public bool SequentialMaps(string current, string next)
    {
        var currentIndex = IndexOf(current);
        if (currentIndex == -1)
            return false;

        var nextIndex = IndexOf(next);
        if (nextIndex == -1)
            return false;

        return currentIndex == nextIndex - 1;
    }

    public int IndexOf(string mapName)
    {
        for (var index = 0; index < Maps.Count; index++)
            if (Maps[index] == mapName)
                return index;

        return -1;
    }
}
