using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;
using L4D2PlayStats.Hub.Core.Infrastructure.Models;

namespace L4D2PlayStats.Hub.Web.Models;

public class PagedListModel<T>
{
    public PagedListModel(Page<T> page)
    {
        Items = page.Items;
        Page = page.Number;
        PageSize = page.Size;
        TotalItems = page.Total;
        TotalPages = page.TotalPages;
    }

    public PagedListModel(IReadOnlyCollection<T> source, int page, int pageSize)
    {
        PageSize = pageSize;
        TotalItems = source.Count;
        TotalPages = Paging.TotalPages(TotalItems, pageSize);
        Page = Paging.Clamp(page, TotalItems, pageSize);
        Items = [.. source.Skip(Paging.Skip(Page, pageSize)).Take(pageSize)];
    }

    private PagedListModel(IReadOnlyCollection<T> items, int page, int pageSize, int totalItems, int totalPages)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalItems = totalItems;
        TotalPages = totalPages;
    }

    public IReadOnlyCollection<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalItems { get; }
    public int TotalPages { get; }
    public int FirstItem => TotalItems == 0 ? 0 : Paging.Skip(Page, PageSize) + 1;
    public int LastItem => Math.Min(Page * PageSize, TotalItems);

    public PagedListModel<TResult> Map<TResult>(Func<T, TResult> selector)
    {
        return new PagedListModel<TResult>([.. Items.Select(selector)], Page, PageSize, TotalItems, TotalPages);
    }
}
