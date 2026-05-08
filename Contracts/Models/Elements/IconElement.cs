namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>
/// 图标元素
/// </summary>
public class IconElement : ElementBase
{
    public string? IconName { get; set; }
    
    public string? IconSet { get; set; }
    
    public string? Color { get; set; }
    
    public double Size { get; set; } = 24;
}
