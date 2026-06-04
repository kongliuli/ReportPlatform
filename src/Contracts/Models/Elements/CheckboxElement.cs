using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Registry;

namespace Xinglin.ReportEditor.Contracts.Models.Elements;

[ElementAdaptationGroup(ElementAdaptationGroup.Form)]
/// <summary>复选框元素</summary>
public class CheckboxElement : ExternalElementBase
{
    /// <summary>是否选中</summary>
    public bool Checked { get; set; }

    /// <summary>选中状态颜色</summary>
    public string? CheckColor { get; set; }

    /// <summary>未选中时的值</summary>
    public string? UncheckedValue { get; set; }

    /// <summary>选中时的值</summary>
    public string? CheckedValue { get; set; }
}
