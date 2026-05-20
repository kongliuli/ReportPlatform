using ReportDataMaker.Models;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Services.ExcelAdapter;

public class TemplateFlattenService
{
    public TemplateFieldSchema Flatten(ExternalTemplateDefinition template)
    {
        var schema = new TemplateFieldSchema
        {
            TemplateName = template.Name,
            TemplateVersion = template.Version
        };

        foreach (var element in template.Elements.OrderBy(e => e.Y).ThenBy(e => e.X))
        {
            if (element.Group != ElementGroup.Editable) continue;
            FlattenElement(element, schema);
        }

        return schema;
    }

    private void FlattenElement(ReportExternalElementBase element, TemplateFieldSchema schema)
    {
        switch (element)
        {
            case ExternalNumberElement number:
                AddField(schema.Fields, element, FieldDataType.Number,
                    minValue: number.MinValue, maxValue: number.MaxValue,
                    decimalPlaces: number.DecimalPlaces);
                break;
            case ExternalDateElement date:
                AddField(schema.Fields, element, FieldDataType.Date, format: date.Format);
                break;
            case ExternalDropdownElement dropdown:
                AddField(schema.Fields, element, FieldDataType.Dropdown, options: dropdown.Options);
                break;
            case ExternalCheckboxElement:
                AddField(schema.Fields, element, FieldDataType.Boolean);
                break;
            case ExternalRadioElement:
                AddField(schema.Fields, element, FieldDataType.Boolean);
                break;
            case ExternalTableElement table:
                FlattenTable(schema, table);
                break;
            default:
                AddField(schema.Fields, element, FieldDataType.Text);
                break;
        }
    }

    private void FlattenTable(TemplateFieldSchema schema, ExternalTableElement table)
    {
        var tableDataPath = !string.IsNullOrEmpty(table.DataPath) ? table.DataPath : table.Id;
        var tableLabel = !string.IsNullOrEmpty(table.Label) ? table.Label : tableDataPath;

        var tableSchema = new TableFieldSchema
        {
            ElementId = table.Id,
            Label = tableLabel,
            DataPath = tableDataPath,
            Rows = table.Rows,
            Columns = table.Columns,
            HeaderRows = table.HeaderRows
        };

        for (int r = 0; r < table.Rows; r++)
        {
            for (int c = 0; c < table.Columns; c++)
            {
                var cellDef = table.Cells?.FirstOrDefault(cd => cd.Row == r && cd.Col == c);
                var isEditable = cellDef?.IsEditable ?? false;
                var isHeaderRow = r < table.HeaderRows;
                var cellText = GetCellText(table, r, c, cellDef);
                var dataType = cellDef?.InputType?.ToLower() switch
                {
                    "number" => FieldDataType.Number,
                    "date" => FieldDataType.Date,
                    "dropdown" => FieldDataType.Dropdown,
                    "boolean" => FieldDataType.Boolean,
                    _ => FieldDataType.Text
                };

                string? cellDataPath = null;
                if (isEditable && !isHeaderRow)
                {
                    cellDataPath = !string.IsNullOrEmpty(cellDef!.DataPath)
                        ? cellDef.DataPath
                        : $"{tableDataPath}.R{r}C{c}";

                    var columnHeader = GetColumnHeader(table, c);
                    var cellLabel = $"{tableLabel} - {columnHeader}";
                    if (table.Rows - table.HeaderRows > 1)
                        cellLabel += $" (行{r - table.HeaderRows + 1})";

                    schema.Fields.Add(new FlatField
                    {
                        DataPath = cellDataPath,
                        Label = cellLabel,
                        DataType = dataType,
                        Options = cellDef?.Options,
                        IsRequired = table.IsRequired,
                        ElementId = table.Id,
                        TableRow = r,
                        TableColumn = c,
                        TableLabel = tableLabel
                    });
                }

                tableSchema.Cells.Add(new TableCellSchema
                {
                    Row = r,
                    Col = c,
                    IsEditable = isEditable,
                    DataPath = cellDataPath,
                    Text = cellText,
                    DataType = dataType,
                    Options = cellDef?.Options
                });
            }
        }

        schema.Tables.Add(tableSchema);
    }

    private static string? GetCellText(ExternalTableElement table, int row, int col, TableCellDefinition? cellDef)
    {
        if (cellDef?.Text != null)
            return cellDef.Text;

        if (table.CellData != null && row < table.CellData.Count && col < table.CellData[row].Count)
            return table.CellData[row][col];

        return null;
    }

    private static string GetColumnHeader(ExternalTableElement table, int colIndex)
    {
        if (table.CellData != null)
        {
            for (int row = 0; row < table.HeaderRows && row < table.CellData.Count; row++)
            {
                if (colIndex < table.CellData[row].Count)
                {
                    var header = table.CellData[row][colIndex]?.Trim();
                    if (!string.IsNullOrEmpty(header))
                        return header;
                }
            }
        }

        if (table.Cells != null)
        {
            var headerCell = table.Cells.FirstOrDefault(c => c.Row < table.HeaderRows && c.Col == colIndex);
            if (headerCell?.Text != null)
                return headerCell.Text.Trim();
        }

        return $"列{colIndex + 1}";
    }

    private void AddField(List<FlatField> fields, ReportExternalElementBase element,
        FieldDataType dataType, string? format = null, List<string>? options = null,
        double? minValue = null, double? maxValue = null, int? decimalPlaces = null)
    {
        fields.Add(new FlatField
        {
            DataPath = !string.IsNullOrEmpty(element.DataPath) ? element.DataPath : element.Id,
            Label = !string.IsNullOrEmpty(element.Label) ? element.Label : element.DataPath ?? element.Id,
            DataType = dataType,
            Format = format,
            Options = options,
            IsRequired = element.IsRequired,
            MinValue = minValue,
            MaxValue = maxValue,
            DecimalPlaces = decimalPlaces,
            ElementId = element.Id
        });
    }
}
