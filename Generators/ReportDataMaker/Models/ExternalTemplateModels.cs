namespace ReportDataMaker.Models;

public class ExternalTemplateDefinition
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Version { get; set; } = "1";
    public string? HospitalId { get; set; }
    public double PageWidth { get; set; } = 210;
    public double PageHeight { get; set; } = 297;
    public string Orientation { get; set; } = "Portrait";
    public double MarginLeft { get; set; } = 20;
    public double MarginRight { get; set; } = 20;
    public double MarginTop { get; set; } = 20;
    public double MarginBottom { get; set; } = 20;
    public string BackgroundColor { get; set; } = "#FFFFFF";
    public double GlobalFontSize { get; set; }
    public bool EnableGlobalFontSize { get; set; }
    public List<ReportExternalElementBase> Elements { get; set; } = new();
    public List<LegacyDataBindingDefinition> DataBindings { get; set; } = new();
    public string? FilePath { get; set; }
}

public class LegacyDataBindingDefinition
{
    public string? Id { get; set; }
    public string ElementId { get; set; } = string.Empty;
    public string DataPath { get; set; } = string.Empty;
    public string BindingType { get; set; } = "Text";
    public string? FormatString { get; set; }
    public string? DefaultValue { get; set; }
}
