namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>线条元素</summary>
public class LineElement : ElementBase
{
    /// <summary>线条颜色</summary>
    public string StrokeColor { get; set; } = "#000000";

    /// <summary>线条宽度</summary>
    public double StrokeWidth { get; set; } = 1;

    /// <summary>起点X坐标</summary>
    public double X1 { get; set; }

    /// <summary>起点Y坐标</summary>
    public double Y1 { get; set; }

    /// <summary>终点X坐标</summary>
    public double X2 { get; set; }

    /// <summary>终点Y坐标</summary>
    public double Y2 { get; set; }
}
