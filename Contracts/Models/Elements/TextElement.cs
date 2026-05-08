using Xinglin.ReportEditor.Contracts.Enums;

namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>文本元素</summary>
public class TextElement : ExternalElementBase
{
    /// <summary>文本内容</summary>
    public string? Text { get; set; }

    /// <summary>字体大小</summary>
    public double FontSize { get; set; } = 14;

    /// <summary>字体粗细</summary>
    public string? FontWeight { get; set; }

    /// <summary>字体族</summary>
    public string? FontFamily { get; set; }

    /// <summary>文字颜色</summary>
    public string TextColor { get; set; } = "#000000";

    /// <summary>文本水平对齐方式</summary>
    public string? TextAlign { get; set; }

    /// <summary>文本垂直对齐方式</summary>
    public string? VerticalAlign { get; set; }

    /// <summary>字体样式</summary>
    public string? FontStyle { get; set; }

    /// <summary>文本装饰</summary>
    public string? TextDecoration { get; set; }

    /// <summary>最大字符长度</summary>
    public int? MaxLength { get; set; }

    /// <summary>占位提示文本</summary>
    public string? Placeholder { get; set; }
}
