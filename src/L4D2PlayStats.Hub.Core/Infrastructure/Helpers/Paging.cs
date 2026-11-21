namespace L4D2PlayStats.Hub.Core.Infrastructure.Helpers;

public static class Paging
{
    public static int TotalPages(int totalItems, int pageSize)
    {
        return Math.Max(1, (int)Math.Ceiling(totalItems / (double)Math.Max(1, pageSize)));
    }

    public static int Clamp(int page, int totalItems, int pageSize)
    {
        return Math.Clamp(page, 1, TotalPages(totalItems, pageSize));
    }

    public static int Skip(int page, int pageSize)
    {
        return (Math.Max(1, page) - 1) * pageSize;
    }
}
