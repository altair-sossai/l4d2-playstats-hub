using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;

namespace L4D2PlayStats.Hub.Core.Infrastructure.Models;

public class Page<T>(IReadOnlyList<T> items, int number, int size, int total)
{
    public IReadOnlyList<T> Items { get; } = items;
    public int Number { get; } = number;
    public int Size { get; } = size;
    public int Total { get; } = total;
    public int TotalPages => Paging.TotalPages(Total, Size);
    public bool HasMore => Number < TotalPages;
    public int FirstItem => Total == 0 ? 0 : Paging.Skip(Number, Size) + 1;
    public int LastItem => Math.Min(Number * Size, Total);

    public static Page<T> Empty(int size)
    {
        return new Page<T>([], 1, size, 0);
    }

    public Page<TResult> Map<TResult>(Func<T, TResult> selector)
    {
        return new Page<TResult>([.. Items.Select(selector)], Number, Size, Total);
    }

    public async Task<Page<TResult>> MapAsync<TResult>(Func<T, Task<TResult>> selector)
    {
        var mapped = new List<TResult>(Items.Count);

        foreach (var item in Items)
            mapped.Add(await selector(item));

        return new Page<TResult>(mapped, Number, Size, Total);
    }
}
