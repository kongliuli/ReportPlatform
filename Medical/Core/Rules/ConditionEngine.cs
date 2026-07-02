using System.Globalization;

namespace Xinglin.Medical.Rules;

public sealed class ConditionEngine
{
    public ConditionDisplay Evaluate(IEnumerable<ConditionRule> rules, IReadOnlyDictionary<string, object?> data)
    {
        foreach (var rule in rules.Where(r => r.IsEnabled).OrderByDescending(r => r.Priority))
        {
            if (!data.TryGetValue(rule.DataPath, out var actual))
                continue;

            if (Matches(actual, rule.Operator, rule.Value))
                return new ConditionDisplay(rule.Color, rule.Mark);
        }

        return ConditionDisplay.Default;
    }

    public static bool Matches(object? actual, ConditionOperator op, string expected)
    {
        var left = actual?.ToString() ?? string.Empty;
        return op switch
        {
            ConditionOperator.Equals => string.Equals(left, expected, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.NotEquals => !string.Equals(left, expected, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.GreaterThan => Compare(left, expected) > 0,
            ConditionOperator.LessThan => Compare(left, expected) < 0,
            ConditionOperator.GreaterThanOrEqual => Compare(left, expected) >= 0,
            ConditionOperator.LessThanOrEqual => Compare(left, expected) <= 0,
            ConditionOperator.Contains => left.Contains(expected, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.StartsWith => left.StartsWith(expected, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.EndsWith => left.EndsWith(expected, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    private static int Compare(string left, string right)
    {
        if (double.TryParse(left, NumberStyles.Any, CultureInfo.InvariantCulture, out var l)
            && double.TryParse(right, NumberStyles.Any, CultureInfo.InvariantCulture, out var r))
            return l.CompareTo(r);

        return string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
    }
}
