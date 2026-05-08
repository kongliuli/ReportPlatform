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
            FlattenElement(element, schema.Fields);
        }

        return schema;
    }

    private void FlattenElement(ReportExternalElementBase element, List<FlatField> fields)
    {
        switch (element)
        {
            case ExternalNumberElement number:
                AddField(fields, element, FieldDataType.Number,
                    minValue: number.MinValue, maxValue: number.MaxValue,
                    decimalPlaces: number.DecimalPlaces);
                break;
            case ExternalDateElement date:
                AddField(fields, element, FieldDataType.Date, format: date.Format);
                break;
            case ExternalDropdownElement dropdown:
                AddField(fields, element, FieldDataType.Dropdown, options: dropdown.Options);
                break;
            case ExternalCheckboxElement:
                AddField(fields, element, FieldDataType.Boolean);
                break;
            case ExternalRadioElement:
                AddField(fields, element, FieldDataType.Boolean);
                break;
            case ExternalTableElement table:
                FlattenTable(table, fields);
                break;
            default:
                AddField(fields, element, FieldDataType.Text);
                break;
        }
    }

    private void FlattenTable(ExternalTableElement table, List<FlatField> fields)
    {
        for (int row = table.HeaderRows; row < table.Rows; row++)
        {
            for (int col = 0; col < table.Columns; col++)
            {
                var cell = table.Cells?.FirstOrDefault(c => c.Row == row && c.Col == col);
                if (cell?.IsEditable == true && !string.IsNullOrEmpty(cell.DataPath))
                {
                    var dataType = cell.InputType?.ToLower() switch
                    {
                        "number" => FieldDataType.Number,
                        "date" => FieldDataType.Date,
                        "dropdown" => FieldDataType.Dropdown,
                        _ => FieldDataType.Text
                    };

                    fields.Add(new FlatField
                    {
                        DataPath = cell.DataPath,
                        Label = cell.Text ?? $"表格[{row}][{col}]",
                        DataType = dataType,
                        Options = cell.Options,
                        IsRequired = false,
                        ElementId = cell.DataPath
                    });
                }
            }
        }
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
