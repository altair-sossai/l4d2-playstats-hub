namespace L4D2PlayStats.Hub.Web.Models.Skins;

public class PagedList<T>
{
    public PagedList(IReadOnlyCollection<T> items, int page, int pageSize)
    {
        Total = items.Count;
        Pages = Math.Max(1, (int)Math.Ceiling(Total / (double)pageSize));
        Page = Math.Clamp(page, 1, Pages);
        Items = [.. items.Skip((Page - 1) * pageSize).Take(pageSize)];
    }

    public IReadOnlyList<T> Items { get; }
    public int Page { get; }
    public int Pages { get; }
    public int Total { get; }
}
