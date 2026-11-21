using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;
using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Infrastructure.Extensions;

public static class QueryablePageExtensions
{
    public static async Task<Page<T>> ToPageAsync<T>(this IQueryable<T> query, int page, int size, CancellationToken cancellationToken = default)
    {
        var total = await query.CountAsync(cancellationToken);
        var number = Paging.Clamp(page, total, size);

        if (total == 0)
            return new Page<T>([], number, size, total);

        var items = await query.Skip(Paging.Skip(number, size)).Take(size).ToListAsync(cancellationToken);

        return new Page<T>(items, number, size, total);
    }
}
