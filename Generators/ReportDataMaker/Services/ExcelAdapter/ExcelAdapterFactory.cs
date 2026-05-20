using ReportDataMaker.Models;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Services.ExcelAdapter;

public class ExcelAdapterFactory
{
    private readonly TemplateFlattenService _flattener = new();
    private readonly ExcelSchemaExporter _exporter = new();
    private readonly ExcelContractReader _reader = new();
    private readonly ExcelDataValidator _validator = new();

    public AdapterType Type => AdapterType.Excel;

    public TemplateFieldSchema FlattenTemplate(ExternalTemplateDefinition template)
        => _flattener.Flatten(template);

    public void ExportTemplate(string filePath, TemplateFieldSchema schema)
        => _exporter.ExportTemplate(filePath, schema);

    public AdapterResult ReadData(string filePath, ExcelTemplateSchema schema)
        => _reader.ReadByContract(filePath, schema);

    public AdapterResult ReadBatchData(string filePath, ExcelTemplateSchema schema)
        => _reader.ReadBatchByContract(filePath, schema);

    public ValidationReport Validate(TemplateFieldSchema schema, List<Dictionary<string, object>> batchData)
        => _validator.Validate(schema, batchData);

    public ExcelTemplateSchema BuildExcelTemplateSchema(TemplateFieldSchema schema)
    {
        var excelSchema = new ExcelTemplateSchema();

        foreach (var field in schema.Fields)
            excelSchema.Fields.Add(field);

        return excelSchema;
    }
}
