namespace Xinglin.ReportEditor.Contracts.Models.Template;

/// <summary>页面设置</summary>
public class PageSettings
{
    /// <summary>页面宽度（毫米）</summary>
    public double PageWidth { get; set; } = 210;

    /// <summary>页面高度（毫米）</summary>
    public double PageHeight { get; set; } = 297;

    /// <summary>左边距</summary>
    public double MarginLeft { get; set; } = 20;

    /// <summary>右边距</summary>
    public double MarginRight { get; set; } = 20;

    /// <summary>上边距</summary>
    public double MarginTop { get; set; } = 20;

    /// <summary>下边距</summary>
    public double MarginBottom { get; set; } = 20;

    /// <summary>页面方向</summary>
    public PageOrientation Orientation { get; set; } = PageOrientation.Portrait;

    /// <summary>背景颜色</summary>
    public string BackgroundColor { get; set; } = "#FFFFFF";
}

/// <summary>页面方向枚举</summary>
public enum PageOrientation
{
    /// <summary>纵向</summary>
    Portrait,

    /// <summary>横向</summary>
    Landscape
}
