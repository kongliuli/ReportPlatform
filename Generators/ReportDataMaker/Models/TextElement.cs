using System.Collections.Generic;

namespace ReportDataMaker.Models;

public class TextElement
{
    public string? Id { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string? Text { get; set; }
    public string? Label { get; set; }
    public double FontSize { get; set; }
    public string? FontWeight { get; set; }
    public string? TextColor { get; set; }
    public string? Align { get; set; }
}
