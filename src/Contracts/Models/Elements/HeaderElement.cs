using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Registry;

namespace Xinglin.ReportEditor.Contracts.Models.Elements;

[ElementAdaptationGroup(ElementAdaptationGroup.Advanced)]
/// <summary>页眉元素</summary>
public class HeaderElement : ElementBase
{
    /// <summary>子元素列表</summary>
    public List<ExternalElementBase> Children { get; set; } = new();

    /// <summary>是否在第一页显示</summary>
    public bool ShowOnFirstPage { get; set; } = true;

    /// <summary>是否在所有页面显示</summary>
    public bool ShowOnAllPages { get; set; }

    /// <summary>页眉内容</summary>
    public string? Content { get; set; }
}
