using ReportDataMaker.Models;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Services.ExcelAdapter;

/// <summary>Excel适配器工厂，提供Excel模板的扁平化、导出、读取和校验功能</summary>
public class ExcelAdapterFactory
{
    private readonly TemplateFlattenService _flattener = new();
    private readonly ExcelSchemaExporter _exporter = new();
    private readonly ExcelContractReader _reader = new();
    private readonly ExcelDataValidator _validator = new();

    /// <summary>适配器类型</summary>
    public AdapterType Type => AdapterType.Excel;

    /// <summary>将外部模板定义扁平化为字段模式</summary>
    /// <param name="template">外部模板定义</param>
    /// <returns>模板字段模式</returns>
    public TemplateFieldSchema FlattenTemplate(ExternalTemplateDefinition template)
        => _flattener.Flatten(template);

    /// <summary>将模板字段模式导出为Excel文件</summary>
    /// <param name="filePath">导出文件路径</param>
    /// <param name="schema">模板字段模式</param>
    public void ExportTemplate(string filePath, TemplateFieldSchema schema)
        => _exporter.ExportTemplate(filePath, schema);

    /// <summary>按契约从Excel文件读取单行数据</summary>
    /// <param name="filePath">Excel文件路径</param>
    /// <param name="schema">Excel模板模式</param>
    /// <returns>适配器结果</returns>
    public AdapterResult ReadData(string filePath, ExcelTemplateSchema schema)
        => _reader.ReadByContract(filePath, schema);

    /// <summary>按契约从Excel文件批量读取数据</summary>
    /// <param name="filePath">Excel文件路径</param>
    /// <param name="schema">Excel模板模式</param>
    /// <returns>适配器结果</returns>
    public AdapterResult ReadBatchData(string filePath, ExcelTemplateSchema schema)
        => _reader.ReadBatchByContract(filePath, schema);

    /// <summary>校验批量数据是否符合模板字段模式</summary>
    /// <param name="schema">模板字段模式</param>
    /// <param name="batchData">批量数据</param>
    /// <returns>校验报告</returns>
    public ValidationReport Validate(TemplateFieldSchema schema, List<Dictionary<string, object>> batchData)
        => _validator.Validate(schema, batchData);
}
