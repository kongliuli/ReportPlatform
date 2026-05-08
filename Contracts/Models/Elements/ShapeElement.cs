namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>形状元素</summary>
public class ShapeElement : ElementBase
{
    /// <summary>形状类型</summary>
    public string ShapeType { get; set; } = "Rectangle";

    /// <summary>填充颜色</summary>
    public string? FillColor { get; set; }

    /// <summary>边框颜色</summary>
    public string? StrokeColor { get; set; }

    /// <summary>边框宽度</summary>
    public double StrokeWidth { get; set; } = 1;

    /// <summary>圆角半径</summary>
    public double? CornerRadius { get; set; }

    /// <summary>不透明度</summary>
    public double? Opacity { get; set; }
}
