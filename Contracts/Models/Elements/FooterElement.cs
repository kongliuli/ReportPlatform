namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>
/// 页脚元素
/// </summary>
public class FooterElement : ElementBase
{
    public List<ExternalElementBase> Children { get; set; } = new();
    
    public bool ShowOnLastPage { get; set; } = true;
    
    public bool ShowOnAllPages { get; set; }
    
    public string? BackgroundColor { get; set; }
    
    public double Height { get; set; } = 30;
}
