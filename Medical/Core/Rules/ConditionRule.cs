namespace Xinglin.Medical.Rules;

public sealed class ConditionRule
{
    public string DataPath { get; set; } = string.Empty;
    public ConditionOperator Operator { get; set; }
    public string Value { get; set; } = string.Empty;
    public string Color { get; set; } = "#000000";
    public string Mark { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public sealed record ConditionDisplay(string Color, string Mark)
{
    public static readonly ConditionDisplay Default = new("#000000", string.Empty);
}
