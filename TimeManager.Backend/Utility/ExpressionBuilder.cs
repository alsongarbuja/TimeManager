using System.Linq.Expressions;

namespace TimeManager.Backend.Utility
{
    public class FilterCondition
    {
        public string PropertyName { get; set; } = string.Empty;
        public FilterOperator Operator { get; set; }
        public string? Value { get; set; }
    }

    public enum FilterOperator
    {
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        Contains,
        StartsWith,
        EndsWith
    }

    public class ExpressionBuilder<T>
    {
        public Expression<Func<T, bool>>? BuildPredicate(FilterCondition filterCondition)
        {
            if (string.IsNullOrEmpty(filterCondition.PropertyName)) return null;
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, filterCondition.PropertyName);
            var constant = BuildConstantExpression(filterCondition.Value, property.Type);

            Expression comparison = filterCondition.Operator switch
            {
                FilterOperator.Equals => Expression.Equal(property, constant),
                FilterOperator.NotEquals => Expression.NotEqual(property, constant),
                FilterOperator.GreaterThan => Expression.GreaterThan(property, constant),
                FilterOperator.LessThan => Expression.LessThan(property, constant),
                FilterOperator.GreaterThanOrEqual =>
                    Expression.GreaterThanOrEqual(property, constant),
                FilterOperator.LessThanOrEqual =>
                    Expression.LessThanOrEqual(property, constant),
                FilterOperator.Contains => BuildContainsExpression(property, filterCondition.Value),
                FilterOperator.StartsWith => BuildStringMethod(property, filterCondition.Value, "StartWith"),
                FilterOperator.EndsWith => BuildStringMethod(property, filterCondition.Value, "EndsWith"),
                _ => throw new NotSupportedException($"Operator {filterCondition.Operator} not supported")
            };

            return Expression.Lambda<Func<T, bool>>(comparison, parameter);
        }

        private Expression BuildContainsExpression(MemberExpression property, object? value)
        {
            if (property.Type != typeof(string))
            {
                throw new NotSupportedException("Contains is only supported for string properties");
            }

            var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var constant = Expression.Constant(value?.ToString() ?? string.Empty);
            return Expression.Call(property, method!, constant);
        }

        private Expression BuildStringMethod(MemberExpression property, object? value, string methodName)
        {
            if (property.Type != typeof(string))
            {
                throw new NotSupportedException($"{methodName} is only supported for string properties");
            }

            var method = typeof(string).GetMethod(methodName, new[] { typeof(string) });
            var constant = Expression.Constant(value?.ToString() ?? string.Empty);
            return Expression.Call(property, method!, constant);
        }

        private Expression BuildConstantExpression(object? value, Type targetType)
        {
            if (value == null)
            {
                if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
                {
                    throw new InvalidOperationException($"Cannot compare non-nullable type {targetType.Name} with null");
                }

                return Expression.Constant(null, targetType);
            }

            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            var converted = Convert.ChangeType(value, underlyingType);
            var constant = Expression.Constant(converted, underlyingType);

            return underlyingType == targetType ? constant : Expression.Convert(constant, targetType);
        }
    }
}
