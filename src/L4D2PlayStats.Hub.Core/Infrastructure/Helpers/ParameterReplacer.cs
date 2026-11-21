using System.Linq.Expressions;

namespace L4D2PlayStats.Hub.Core.Infrastructure.Helpers;

public sealed class ParameterReplacer(ParameterExpression from, Expression to) : ExpressionVisitor
{
    public static Expression Replace(LambdaExpression lambda, Expression to)
    {
        return new ParameterReplacer(lambda.Parameters[0], to).Visit(lambda.Body);
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return node == from ? to : base.VisitParameter(node);
    }
}
