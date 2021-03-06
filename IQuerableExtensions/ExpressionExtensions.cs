using System.Linq.Expressions;
using System.Reflection;

namespace IQuerableExtensions;

public static partial class ExpressionExtensions
{

    /// <summary>
    /// Get Condition Expression
    /// </summary>
    /// <returns> <c><paramref name="parameter"/>.key1 == <paramref name="obj"/>.key1 And <paramref name="parameter"/>.key2 == ... </c> </returns>
    public static Expression? GetEquelCondition(this ParameterExpression parameter, object? obj, IEnumerable<PropertyInfo> keys)
    {
        if (obj == null || !keys.Any())
            return null;
        var entityConst = Expression.Constant(obj);
        Expression? condition = null;
        foreach (var key in keys)
        {
            var paramEquals = Expression.Equal(Expression.Property(parameter, key.Name), Expression.Property(entityConst, key.Name));
            condition = condition.AndAlso(paramEquals);
        }
        return condition;
    }

    public static Expression? AndAlso(this Expression? left, Expression? right)
    {
        if (left == null)
            return right;
        else if (right != null)
            return Expression.AndAlso(left, right);
        else return null;
    }

    public static Expression? OrElse(this Expression? left, Expression? right)
    {
        if (left == null)
            return right;
        else if (right != null)
            return Expression.OrElse(left, right);
        else return null;
    }

    public static Expression ReplaceParameter(this Expression expression, ParameterExpression from, Expression to)
    {
        if (!to.Type.IsAssignableTo(from.Type))
            throw new ArgumentException($"{to.Type} cannot be assigned to {from.Type.Name}", nameof(to));
        return ReplaceParameters(expression, new() { [from] = to });
    }

    public static Expression ReplaceParameters(this Expression expression, IEnumerable<ParameterExpression> from, IEnumerable<Expression> to)
    {
        var zip = from.Zip(to);
        if (zip.Any(e => !e.Second.Type.IsAssignableTo(e.First.Type)))
        {
            throw new ArgumentException($"Some of {nameof(to)} cannot be assigned.", nameof(to));
        }
        var dic = zip.ToDictionary(x => x.First, x => x.Second);
        return ReplaceParameters(expression, dic);
    }

    public static Expression ReplaceParameters(this Expression expression, Dictionary<ParameterExpression, Expression> pairs)
    {
        return new ParameterReplaceVisitor(pairs).Visit(expression);
    }
}
