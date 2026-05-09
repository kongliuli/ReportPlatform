using Xinglin.ReportEditor.Contracts.Enums;

namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>文本元素</summary>
public class TextElement : ExternalElementBase
{
    /// <summary>文本内容</summary>
    public string? Text { get; set; }

    /// <summary>文字颜色（兼容旧版，推荐使用 ForegroundColor）</summary>
    public string TextColor { get; set; } = "#000000";

    /// <summary>文本垂直对齐方式</summary>
    public string? VerticalAlign { get; set; }

    /// <summary>文本装饰</summary>
    public string? TextDecoration { get; set; }

    /// <summary>最大字符长度</summary>
    public int? MaxLength { get; set; }

    /// <summary>占位提示文本</summary>
    public string? Placeholder { get; set; }

    /// <summary>行高倍数</summary>
    public double LineHeight { get; set; } = 1.5;

    /// <summary>字间距（px）</summary>
    public double LetterSpacing { get; set; }
}
