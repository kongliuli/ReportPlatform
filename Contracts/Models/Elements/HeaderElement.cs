namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>
/// 页眉元素
/// </summary>
public class HeaderElement : ElementBase
{
    public List<ExternalElementBase> Children { get; set; } = new();
    
    public bool ShowOnFirstPage { get; set; } = true;
    
    public bool ShowOnAllPages { get; set; }
    
    public string? BackgroundColor { get; set; }
    
    public double Height { get; set; } = 30;
}
