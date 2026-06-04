using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Registry;

namespace Xinglin.ReportEditor.Contracts.Models.Elements;

[ElementAdaptationGroup(ElementAdaptationGroup.Basic)]
/// <summary>形状元素</summary>
public class ShapeElement : ElementBase
{
    /// <summary>形状类型</summary>
    public string ShapeType { get; set; } = "Rectangle";

    /// <summary>填充颜色</summary>
    public string? FillColor { get; set; }

    /// <summary>边框颜色（兼容旧版，推荐使用 BorderColor）</summary>
    public string? StrokeColor { get; set; }

    /// <summary>边框宽度（兼容旧版，推荐使用 BorderWidth）</summary>
    public double StrokeWidth { get; set; } = 1;
}
