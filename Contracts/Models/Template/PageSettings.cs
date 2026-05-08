namespace Xinglin.ReportEditor.Contracts.Models.Template;

/// <summary>
/// 页面设置
/// </summary>
public class PageSettings
{
    public double PageWidth { get; set; } = 210;
    
    public double PageHeight { get; set; } = 297;
    
    public double MarginLeft { get; set; } = 20;
    
    public double MarginRight { get; set; } = 20;
    
    public double MarginTop { get; set; } = 20;
    
    public double MarginBottom { get; set; } = 20;
    
    public PageOrientation Orientation { get; set; } = PageOrientation.Portrait;
    
    public string BackgroundColor { get; set; } = "#FFFFFF";
}

/// <summary>
/// 页面方向
/// </summary>
public enum PageOrientation
{
    Portrait,
    Landscape
}
