using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Services.ExcelAdapter;

public class FlatField : FieldSchema
{
    public string? ElementId { get; set; }

    public int? TableRow { get; set; }

    public int? TableColumn { get; set; }

    public string? TableLabel { get; set; }
}

public class TableCellSchema
{
    public int Row { get; set; }
    public int Col { get; set; }
    public bool IsEditable { get; set; }
    public string? DataPath { get; set; }
    public string? Text { get; set; }
    public FieldDataType DataType { get; set; } = FieldDataType.Text;
    public List<string>? Options { get; set; }
}

public class TableFieldSchema
{
    public string ElementId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string DataPath { get; set; } = string.Empty;
    public int Rows { get; set; }
    public int Columns { get; set; }
    public int HeaderRows { get; set; }
    public List<TableCellSchema> Cells { get; set; } = new();
}

public class TemplateFieldSchema
{
    public string TemplateName { get; set; } = string.Empty;
    public string TemplateVersion { get; set; } = string.Empty;
    public List<FlatField> Fields { get; set; } = new();
    public List<TableFieldSchema> Tables { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}
