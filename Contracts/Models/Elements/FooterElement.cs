namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>页脚元素</summary>
public class FooterElement : ElementBase
{
    /// <summary>子元素列表</summary>
    public List<ExternalElementBase> Children { get; set; } = new();

    /// <summary>是否在最后一页显示</summary>
    public bool ShowOnLastPage { get; set; } = true;

    /// <summary>是否在所有页面显示</summary>
    public bool ShowOnAllPages { get; set; }

    /// <summary>页脚内容</summary>
    public string? Content { get; set; }

    /// <summary>是否显示页码</summary>
    public bool ShowPageNumber { get; set; }
}
