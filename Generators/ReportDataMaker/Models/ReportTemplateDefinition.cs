using System.Collections.Generic;

namespace ReportDataMaker.Models;

public class ReportTemplateDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string HospitalId { get; set; } = string.Empty;
    public double PageWidth { get; set; }
    public double PageHeight { get; set; }
    public double MarginLeft { get; set; }
    public double MarginRight { get; set; }
    public double MarginTop { get; set; }
    public double MarginBottom { get; set; }
    public string Orientation { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = string.Empty;
    public List<ElementBase> Elements { get; set; } = new();
}
