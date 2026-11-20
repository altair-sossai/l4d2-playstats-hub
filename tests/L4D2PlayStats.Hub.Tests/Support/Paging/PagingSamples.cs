namespace L4D2PlayStats.Hub.Tests.Support.Paging;

public static class PagingSamples
{
    public const long FirstCommunityId = 76561198000010000;

    public static long CommunityId(int index)
    {
        return FirstCommunityId + index;
    }

    public static string Name(int index)
    {
        return $"Ranked{index:000}";
    }
}