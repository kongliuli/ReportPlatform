namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>
/// 容器元素，用于嵌套子元素
/// </summary>
public class ContainerElement : ElementBase
{
    public List<ExternalElementBase> Children { get; set; } = new();
    
    public string? Layout { get; set; }
    
    public double Padding { get; set; }
    
    public bool ClipContent { get; set; }
    
    public string? BackgroundColor { get; set; }
    
    public string? BorderColor { get; set; }
    
    public double? BorderWidth { get; set; }
}
