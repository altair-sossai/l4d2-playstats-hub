using System.Globalization;
using System.Linq.Expressions;
using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Models;

public class PlayerSearch
{
    private static readonly PlayerSearch None = new(null);

    private PlayerSearch(string? term)
    {
        Term = string.IsNullOrEmpty(term) ? null : term;
        Lower = Term?.ToLowerInvariant() ?? string.Empty;
        CommunityId = SteamIdParser.Parse(Term);
    }

    public string? Term { get; }
    public string Lower { get; }
    public long? CommunityId { get; }
    public bool IsEmpty => Term == null;

    public static PlayerSearch Of(string? value)
    {
        var term = value?.Trim();

        return string.IsNullOrEmpty(term) ? None : new PlayerSearch(term);
    }

    public bool Matches(long communityId, params IEnumerable<string?> names)
    {
        if (Term == null)
            return true;

        return communityId == CommunityId
               || communityId.ToString(CultureInfo.InvariantCulture).Contains(Term, StringComparison.Ordinal)
               || names.Any(name => name != null && name.Contains(Term, StringComparison.OrdinalIgnoreCase));
    }

    public IQueryable<T> Apply<T>(IQueryable<T> query, Expression<Func<T, long>> communityId, params Expression<Func<T, string?>>[] names)
    {
        return Term == null ? query : query.Where(Filter(communityId, names));
    }

    public Expression<Func<T, bool>> Filter<T>(Expression<Func<T, long>> communityId, params Expression<Func<T, string?>>[] names)
    {
        var parameter = communityId.Parameters[0];

        if (Term == null)
            return Expression.Lambda<Func<T, bool>>(Expression.Constant(true), parameter);

        var self = Expression.Constant(this);
        var id = communityId.Body;
        var idText = Expression.Call(id, typeof(long).GetMethod(nameof(ToString), Type.EmptyTypes)!);
        var contains = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
        var toLower = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;

        Expression body = Expression.Call(idText, contains, Expression.Property(self, nameof(Term)));

        if (CommunityId != null)
            body = Expression.OrElse(Expression.Equal(id, Expression.Convert(Expression.Property(self, nameof(CommunityId)), typeof(long))), body);

        foreach (var name in names)
        {
            var value = ParameterReplacer.Replace(name, parameter);
            var matches = Expression.AndAlso(
                Expression.NotEqual(value, Expression.Constant(null, typeof(string))),
                Expression.Call(Expression.Call(value, toLower), contains, Expression.Property(self, nameof(Lower))));

            body = Expression.OrElse(body, matches);
        }

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
