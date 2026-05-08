namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>
/// 形状元素（矩形、椭圆等）
/// </summary>
public class ShapeElement : ElementBase
{
    public string ShapeType { get; set; } = "Rectangle";
    
    public string? FillColor { get; set; }
    
    public string? StrokeColor { get; set; }
    
    public double StrokeWidth { get; set; } = 1;
    
    public double? CornerRadius { get; set; }
    
    public double? Opacity { get; set; }
}
