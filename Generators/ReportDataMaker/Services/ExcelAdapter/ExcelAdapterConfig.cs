using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Services.ExcelAdapter;

/// <summary>Excel适配器配置，定义Excel数据导入的配置信息</summary>
public class ExcelAdapterConfig : AdapterConfigBase
{
    /// <summary>Excel文件路径</summary>
    public string? FilePath { get; set; }
    /// <summary>工作表名称</summary>
    public string? SheetName { get; set; }
    /// <summary>已导出的模板模式</summary>
    public ExcelTemplateSchema ExportedSchema { get; set; } = new();
    /// <summary>导入模式</summary>
    public ImportMode Mode { get; set; } = ImportMode.Single;
}

/// <summary>Excel模板模式，定义Excel文件中各行的含义</summary>
public class ExcelTemplateSchema
{
    /// <summary>契约行号</summary>
    public int ContractRow { get; set; } = 1;
    /// <summary>标签行号</summary>
    public int LabelRow { get; set; } = 2;
    /// <summary>类型行号</summary>
    public int TypeRow { get; set; } = 3;
    /// <summary>数据起始行号</summary>
    public int DataStartRow { get; set; } = 4;
    /// <summary>字段列表</summary>
    public List<FlatField> Fields { get; set; } = new();
}

/// <summary>导入模式枚举</summary>
public enum ImportMode
{
    /// <summary>单条导入</summary>
    Single,
    /// <summary>批量导入</summary>
    Batch
}
