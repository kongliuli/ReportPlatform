namespace Xinglin.ReportEditor.Contracts.Models.Elements;

public class DividerElement : ElementBase
{
    public double Thickness { get; set; } = 1;
    
    public string? Color { get; set; }
    
    public string? Style { get; set; }
    
    public double MarginTop { get; set; }
    
    public double MarginBottom { get; set; }
}
