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

    /// <summary>背景颜色</summary>
    public string? BackgroundColor { get; set; }

    /// <summary>页脚高度</summary>
    public double Height { get; set; } = 30;
}
