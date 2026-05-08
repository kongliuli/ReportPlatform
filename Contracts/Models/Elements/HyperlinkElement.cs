namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>
/// 超链接元素
/// </summary>
public class HyperlinkElement : ExternalElementBase
{
    public string? Url { get; set; }
    
    public bool OpenInNewTab { get; set; }
    
    public string? Text { get; set; }
    
    public string? Color { get; set; }
    
    public string? Underline { get; set; }
}
