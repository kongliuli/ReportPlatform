using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Adapter.Excel.Services;

public class ExcelAdapterConfig : AdapterConfigBase
{
    public string? FilePath { get; set; }
    public string? SheetName { get; set; }
    public ExcelTemplateSchema ExportedSchema { get; set; } = new();
    public ImportMode Mode { get; set; } = ImportMode.Single;
}

public class ExcelTemplateSchema
{
    public int ContractRow { get; set; } = 1;
    public int LabelRow { get; set; } = 2;
    public int TypeRow { get; set; } = 3;
    public int DataStartRow { get; set; } = 4;
    public List<FlatField> Fields { get; set; } = new();
}

public enum ImportMode
{
    Single,
    Batch
}
