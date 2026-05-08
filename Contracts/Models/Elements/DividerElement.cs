namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>
/// 分隔线元素
/// </summary>
public class DividerElement : ElementBase
{
    public double Thickness { get; set; } = 1;
    
    public string? Color { get; set; }
    
    public string? Style { get; set; }
    
    public double MarginTop { get; set; }
    
    public double MarginBottom { get; set; }
}
