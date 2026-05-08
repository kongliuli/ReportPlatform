namespace ReportDataMaker.Models;

public class ElementBase
{
    public string Type { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public bool IsVisible { get; set; } = true;
    public double Rotation { get; set; }
    public int ZIndex { get; set; }
    public string BorderColor { get; set; } = string.Empty;
    public double BorderWidth { get; set; }
    public string BorderStyle { get; set; } = string.Empty;
    public double CornerRadius { get; set; }
    public double Opacity { get; set; } = 1;
    public string ShadowColor { get; set; } = string.Empty;
    public double ShadowDepth { get; set; }
    public string HorizontalAlignment { get; set; } = string.Empty;
    public string VerticalAlignment { get; set; } = string.Empty;
}
