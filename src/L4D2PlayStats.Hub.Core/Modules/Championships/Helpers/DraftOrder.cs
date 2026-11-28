namespace L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;

public static class DraftOrder
{
    public static int Position(int pickIndex, int teams, bool snake)
    {
        if (teams <= 0)
            return 0;

        var round = pickIndex / teams;
        var offset = pickIndex % teams;

        return snake && round % 2 == 1 ? teams - 1 - offset : offset;
    }

    public static int Round(int pickIndex, int teams)
    {
        return teams <= 0 ? 1 : pickIndex / teams + 1;
    }
}
