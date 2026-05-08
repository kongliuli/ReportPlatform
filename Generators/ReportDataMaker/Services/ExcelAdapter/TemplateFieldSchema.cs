using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Services.ExcelAdapter;

public class FlatField : FieldSchema
{
    public string? ElementId { get; set; }
}

public class TemplateFieldSchema
{
    public string TemplateName { get; set; } = string.Empty;
    public string TemplateVersion { get; set; } = string.Empty;
    public List<FlatField> Fields { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}
