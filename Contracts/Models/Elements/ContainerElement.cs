namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>容器元素</summary>
public class ContainerElement : ElementBase
{
    /// <summary>子元素列表</summary>
    public List<ExternalElementBase> Children { get; set; } = new();

    /// <summary>布局方式</summary>
    public string? Layout { get; set; }

    /// <summary>内边距</summary>
    public double Padding { get; set; }

    /// <summary>是否裁剪内容</summary>
    public bool ClipContent { get; set; }

    /// <summary>背景颜色</summary>
    public string? BackgroundColor { get; set; }

    /// <summary>边框颜色</summary>
    public string? BorderColor { get; set; }

    /// <summary>边框宽度</summary>
    public double? BorderWidth { get; set; }
}
