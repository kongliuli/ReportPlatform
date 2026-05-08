using Xinglin.ReportEditor.Contracts.Enums;

namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>
/// 文本元素，支持标签和输入
/// </summary>
public class TextElement : ExternalElementBase
{
    public string? Text { get; set; }
    
    public double FontSize { get; set; } = 14;
    
    public string? FontWeight { get; set; }
    
    public string? FontFamily { get; set; }
    
    public string TextColor { get; set; } = "#000000";
    
    public string? TextAlign { get; set; }
    
    public string? VerticalAlign { get; set; }
    
    public string? FontStyle { get; set; }
    
    public string? TextDecoration { get; set; }
    
    public int? MaxLength { get; set; }
    
    public string? Placeholder { get; set; }
}
