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

        public Expression<Func<T, bool>>? BuildPredicate(FilterCondition filter)
        {
            if (string.IsNullOrEmpty(filter.PropertyName)) return null;
            var parameter = Expression.Parameter(typeof(T), "x");
            var comparision = BuildComparision(filter, parameter);

            return Expression.Lambda<Func<T, bool>>(comparision, parameter);
        }

        private Expression BuildComparision(FilterCondition filter, ParameterExpression parameter)
        {
            Expression property = parameter;

            foreach (var propName in filter.PropertyName.Split("."))
            {
                property = Expression.Property(property, propName);
            }
            
            var constant = BuildConstantExpression(filter.Value, property.Type);

            return filter.Operator switch
            {
                FilterOperator.Equals => Expression.Equal(property, constant),
                FilterOperator.NotEquals => Expression.NotEqual(property, constant),
                FilterOperator.GreaterThan => Expression.GreaterThan(property, constant),
                FilterOperator.LessThan => Expression.LessThan(property, constant),
                FilterOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(property, constant),
                FilterOperator.LessThanOrEqual => Expression.LessThanOrEqual(property, constant),
                FilterOperator.Contains => BuildContainsExpression(property, filter.Value),
                FilterOperator.StartsWith => BuildStringMethod(property, filter.Value, "StartsWith"),
                FilterOperator.EndsWith => BuildStringMethod(property, filter.Value, "EndsWith"),
                _ => throw new NotSupportedException($"Operator {filter.Operator} not supported")
            };
        }

        public Expression<Func<T, bool>>? BuildPredicate(IEnumerable<FilterCondition> filters, bool useAnd = true)
        {
            var validConditions = filters?.Where(c => !string.IsNullOrEmpty(c.PropertyName)).ToList();
            if (validConditions == null || validConditions.Count == 0) return null;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? combinedExpression = null;

            foreach (var condition in validConditions)
            {
                var comparision = BuildComparision(condition, parameter);

                if (combinedExpression == null)
                {
                    combinedExpression = comparision;
                }
                else
                {
                    combinedExpression = useAnd
                        ? Expression.AndAlso(combinedExpression, comparision)
                        : Expression.OrElse(combinedExpression, comparision);
                }
            }

            return combinedExpression == null ? null : Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
        }

        private Expression BuildContainsExpression(Expression property, object? value)
        {
            if (property.Type != typeof(string))
            {
                throw new NotSupportedException("Contains is only supported for string properties");
            }

            var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var constant = Expression.Constant(value?.ToString() ?? string.Empty);
            return Expression.Call(property, method!, constant);
        }

        private Expression BuildStringMethod(Expression property, object? value, string methodName)
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
