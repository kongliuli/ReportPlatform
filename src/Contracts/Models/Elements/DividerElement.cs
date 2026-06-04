using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Registry;

namespace Xinglin.ReportEditor.Contracts.Models.Elements;

[ElementAdaptationGroup(ElementAdaptationGroup.Basic)]
/// <summary>分割线元素</summary>
public class DividerElement : ElementBase
{
    /// <summary>分割线粗细</summary>
    public double Thickness { get; set; } = 1;

    /// <summary>分割线颜色</summary>
    public string? Color { get; set; }

    /// <summary>分割线样式</summary>
    public string? Style { get; set; }

    /// <summary>上边距</summary>
    public double MarginTop { get; set; }

    /// <summary>下边距</summary>
    public double MarginBottom { get; set; }
}
