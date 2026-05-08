namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>超链接元素</summary>
public class HyperlinkElement : ExternalElementBase
{
    /// <summary>链接地址</summary>
    public string? Url { get; set; }

    /// <summary>是否在新标签页打开</summary>
    public bool OpenInNewTab { get; set; }

    /// <summary>链接显示文本</summary>
    public string? Text { get; set; }

    /// <summary>链接颜色</summary>
    public string? Color { get; set; }

    /// <summary>下划线样式</summary>
    public string? Underline { get; set; }
}
