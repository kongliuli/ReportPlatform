namespace Xinglin.ReportEditor.Contracts.Models.Elements;

public class HyperlinkElement : ExternalElementBase
{
    public string? Url { get; set; }
    
    public bool OpenInNewTab { get; set; }
    
    public string? Text { get; set; }
    
    public string? Color { get; set; }
    
    public string? Underline { get; set; }
}
