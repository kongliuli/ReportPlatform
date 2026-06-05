using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Adapter.Excel.Services;

public class TemplateFlattenService : IDataAdapter
{
    private TemplateDefinition? _template;
    private string? _filePath;
    private ExcelTemplateSchema? _schema;
    private readonly ExcelContractReader _contractReader = new();

    public string AdapterId => "excel-flatten";
    public string AdapterName => "Excel 模板扁平化适配器";
    public AdapterType Type => AdapterType.Excel;
    public IReadOnlyList<string> TargetDataPaths => Array.Empty<string>();

    public Task<AdapterResult> ReadDataAsync()
    {
        if (!string.IsNullOrEmpty(_filePath) && _schema != null)
        {
            var result = _contractReader.ReadByContract(_filePath, _schema);
            return Task.FromResult(result);
        }

        var fields = FlattenTemplate(_template!);
        var data = new Dictionary<string, object>();
        foreach (var field in fields)
        {
            data[field.DataPath] = field;
        }
        return Task.FromResult(new AdapterResult { Success = true, Data = data });
    }

    public Task<AdapterResult> ReadBatchDataAsync()
    {
        if (!string.IsNullOrEmpty(_filePath) && _schema != null)
        {
            var result = _contractReader.ReadBatchByContract(_filePath, _schema);
            return Task.FromResult(result);
        }

        return Task.FromResult(new AdapterResult { Success = false, ErrorMessage = "批量读取需要设置文件路径和模板架构" });
    }

    public Task<ValidationResult> ValidateConfigAsync() => Task.FromResult(ValidationResult.Success);

    public void SetTemplate(TemplateDefinition template) => _template = template;

    public void SetFileConfig(string filePath, ExcelTemplateSchema schema)
    {
        _filePath = filePath;
        _schema = schema;
    }

    public List<FlatField> FlattenTemplate(TemplateDefinition template)
    {
        var fields = new List<FlatField>();
        if (template?.Elements == null) return fields;

        foreach (var element in template.Elements)
        {
            FlattenElement(element, fields);
        }

        return fields;
    }

    private void FlattenElement(ElementBase element, List<FlatField> fields)
    {
        switch (element)
        {
            case TextElement te:
                AddField(fields, te, FieldDataType.Text, te.Text ?? te.DefaultValue ?? string.Empty);
                break;
            case NumberElement ne:
                AddField(fields, ne, FieldDataType.Number, ne.Value ?? ne.DefaultValue ?? string.Empty);
                break;
            case DateElement de:
                AddField(fields, de, FieldDataType.Date, de.Value ?? de.DefaultValue ?? string.Empty);
                break;
            case DropdownElement dd:
                AddField(fields, dd, FieldDataType.Dropdown, dd.SelectedValue ?? dd.DefaultValue ?? string.Empty);
                break;
            case CheckboxElement cb:
                AddField(fields, cb, FieldDataType.Boolean, cb.Checked ? "true" : "false");
                break;
            case RadioElement re:
                AddField(fields, re, FieldDataType.Boolean, re.IsChecked ? "true" : "false");
                break;
            case TableElement tb:
                AddTableField(fields, tb);
                break;
            case SignatureElement sg:
                AddField(fields, sg, FieldDataType.Signature, sg.SignatureData ?? string.Empty);
                break;
            case ContainerElement ct:
                foreach (var child in ct.Children)
                    FlattenElement(child, fields);
                break;
            case HeaderElement hd:
                foreach (var child in hd.Children)
                    FlattenElement(child, fields);
                break;
            case FooterElement ft:
                foreach (var child in ft.Children)
                    FlattenElement(child, fields);
                break;
            case RepeatElement rp:
                AddField(fields, rp, FieldDataType.List, rp.ItemTemplate ?? string.Empty);
                break;
        }
    }

    private static void AddField(List<FlatField> fields, ElementBase element, FieldDataType dataType, string value)
    {
        var extElem = element as ExternalElementBase;
        if (string.IsNullOrEmpty(element.DataPath) && (extElem == null || extElem.Group != ElementGroup.Editable)) return;

        fields.Add(new FlatField
        {
            ElementId = element.Id,
            DataPath = element.DataPath ?? element.Id,
            Label = element.Label ?? element.Id,
            DataType = dataType,
            IsRequired = extElem?.IsRequired ?? false
        });
    }

    private static void AddTableField(List<FlatField> fields, TableElement table)
    {
        var tableDataPath = !string.IsNullOrEmpty(table.DataPath) ? table.DataPath : table.Id;

        if (table.Cells != null)
        {
            foreach (var cell in table.Cells.Where(c => c.IsEditable))
            {
                var cellDataPath = !string.IsNullOrEmpty(cell.DataPath)
                    ? cell.DataPath
                    : $"{tableDataPath}.R{cell.Row}C{cell.Col}";

                var cellValue = table.CellData != null
                    && cell.Row < table.CellData.Count
                    && cell.Col < table.CellData[cell.Row].Count
                    ? table.CellData[cell.Row][cell.Col]
                    : string.Empty;

                fields.Add(new FlatField
                {
                    ElementId = $"{table.Id}_R{cell.Row}C{cell.Col}",
                    DataPath = cellDataPath,
                    Label = cell.Text ?? $"R{cell.Row + 1}C{cell.Col + 1}",
                    DataType = FieldDataType.Text,
                    IsRequired = false,
                    TableRow = cell.Row,
                    TableColumn = cell.Col
                });
            }
        }

        fields.Add(new FlatField
        {
            ElementId = table.Id,
            DataPath = tableDataPath,
            Label = table.Label ?? table.Id,
            DataType = FieldDataType.Table,
            IsRequired = table.IsRequired,
            TableRows = table.Rows,
            TableHeaderRows = table.HasHeader ? table.HeaderRows : 0
        });
    }
}
