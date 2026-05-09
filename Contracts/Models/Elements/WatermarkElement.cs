namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>水印元素</summary>
public class WatermarkElement : ElementBase
{
    /// <summary>水印文本</summary>
    public string? Text { get; set; }

    /// <summary>水印角度</summary>
    public double Angle { get; set; } = -45;

    /// <summary>水印颜色（兼容旧版，推荐使用 ForegroundColor）</summary>
    public string? Color { get; set; }

    /// <summary>是否重复铺满</summary>
    public bool Repeat { get; set; } = true;
}
