namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>
/// 线条元素
/// </summary>
public class LineElement : ElementBase
{
    public string StrokeColor { get; set; } = "#000000";
    
    public double StrokeWidth { get; set; } = 1;
    
    public double X1 { get; set; }
    
    public double Y1 { get; set; }
    
    public double X2 { get; set; }
    
    public double Y2 { get; set; }
}
