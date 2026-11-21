using System.Linq.Expressions;
using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;

namespace L4D2PlayStats.Hub.Core.Infrastructure.Extensions;

public static class PredicateExtensions
{
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        var parameter = left.Parameters[0];

        return Expression.Lambda<Func<T, bool>>(Expression.OrElse(left.Body, ParameterReplacer.Replace(right, parameter)), parameter);
    }
}
