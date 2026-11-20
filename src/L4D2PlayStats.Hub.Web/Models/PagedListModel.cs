using L4D2PlayStats.Hub.Web.Helpers;

namespace L4D2PlayStats.Hub.Web.Models;

public class PagedListModel<T>
{
    public PagedListModel(IReadOnlyCollection<T> source, int page, int pageSize)
        : this(page, pageSize, source.Count)
    {
        Items = [.. source.Skip(Paging.Skip(Page, pageSize)).Take(pageSize)];
    }

    public PagedListModel(IReadOnlyCollection<T> items, int page, int pageSize, int totalItems)
        : this(page, pageSize, totalItems)
    {
        Items = items;
    }

    private PagedListModel(int page, int pageSize, int totalItems)
    {
        PageSize = pageSize;
        TotalItems = totalItems;
        TotalPages = Paging.TotalPages(totalItems, pageSize);
        Page = Paging.Page(page, totalItems, pageSize);
    }

    public IReadOnlyCollection<T> Items { get; private set; } = [];
    public int Page { get; }
    public int PageSize { get; }
    public int TotalItems { get; }
    public int TotalPages { get; }
    public int FirstItem => TotalItems == 0 ? 0 : Paging.Skip(Page, PageSize) + 1;
    public int LastItem => Math.Min(Page * PageSize, TotalItems);
}
