namespace Xinglin.ReportEditor.Contracts.Models.Elements;

public class NumberElement : ExternalElementBase
{
    public string? Value { get; set; }
    
    public double? MinValue { get; set; }
    
    public double? MaxValue { get; set; }
    
    public int DecimalPlaces { get; set; } = 2;
    
    public string? Unit { get; set; }
    
    public string? Prefix { get; set; }
    
    public string? Suffix { get; set; }
}
