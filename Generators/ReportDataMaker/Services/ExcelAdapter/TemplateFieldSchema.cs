using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.Services.ExcelAdapter;

public class FlatField
{
    public string DataPath { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public FieldDataType DataType { get; set; } = FieldDataType.Text;
    public string? Format { get; set; }
    public List<string>? Options { get; set; }
    public bool IsRequired { get; set; }
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public int? DecimalPlaces { get; set; }
    public string? ElementId { get; set; }
}

public class TemplateFieldSchema
{
    public string TemplateName { get; set; } = string.Empty;
    public string TemplateVersion { get; set; } = string.Empty;
    public List<FlatField> Fields { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}
