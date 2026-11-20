namespace L4D2PlayStats.Hub.Web.Helpers;

public static class Paging
{
    public static int TotalPages(int totalItems, int pageSize)
    {
        return Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
    }

    public static int Page(int page, int totalItems, int pageSize)
    {
        return Math.Clamp(page, 1, TotalPages(totalItems, pageSize));
    }

    public static int Skip(int page, int pageSize)
    {
        return (page - 1) * pageSize;
    }
}