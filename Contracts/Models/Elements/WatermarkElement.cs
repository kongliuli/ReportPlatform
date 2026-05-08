namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>
/// 水印元素
/// </summary>
public class WatermarkElement : ElementBase
{
    public string? Text { get; set; }
    
    public double Angle { get; set; } = -45;
    
    public string? Color { get; set; }
    
    public bool Repeat { get; set; } = true;
    
    public double Opacity { get; set; } = 0.3;
    
    public string? FontFamily { get; set; }
    
    public double FontSize { get; set; } = 48;
}
