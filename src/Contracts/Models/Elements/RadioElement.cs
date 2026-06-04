using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Registry;

namespace Xinglin.ReportEditor.Contracts.Models.Elements;

[ElementAdaptationGroup(ElementAdaptationGroup.Form)]
/// <summary>单选按钮元素</summary>
public class RadioElement : ExternalElementBase
{
    /// <summary>单选组名称</summary>
    public string? GroupName { get; set; }

    /// <summary>单选按钮值</summary>
    public string? Value { get; set; }

    /// <summary>是否选中</summary>
    public bool IsChecked { get; set; }
}
