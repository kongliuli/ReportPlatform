namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>图标元素</summary>
public class IconElement : ElementBase
{
    /// <summary>图标名称</summary>
    public string? IconName { get; set; }

    /// <summary>图标集</summary>
    public string? IconSet { get; set; }

    /// <summary>图标颜色</summary>
    public string? Color { get; set; }

    /// <summary>图标大小</summary>
    public double Size { get; set; } = 24;
}
