using ReportDataMaker.Models;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Services;

public class DataBindingService : IDataBindingService
{
    public void ApplyData(ExternalTemplateDefinition template, Dictionary<string, object> data)
    {
        if (template.Elements == null) return;

        foreach (var element in template.Elements)
        {
            if (string.IsNullOrEmpty(element.DataPath) || !data.TryGetValue(element.DataPath, out var value))
                continue;

            if (element is ExternalTableElement table && TryApplyTableData(table, value))
                continue;

            element.DefaultValue = value?.ToString() ?? string.Empty;
        }
    }

    private static bool TryApplyTableData(ExternalTableElement table, object value)
    {
        switch (value)
        {
            case List<List<string>> cellData:
                table.CellData = cellData;
                return true;
            case TableDataValue tableData:
                table.CellData = tableData.Rows;
                return true;
            default:
                return false;
        }
    }

    public Dictionary<string, object> ExtractData(ExternalTemplateDefinition template)
    {
        var data = new Dictionary<string, object>();
        if (template.Elements == null) return data;

        foreach (var element in template.Elements)
        {
            if (string.IsNullOrEmpty(element.DataPath))
                continue;

            if (element is ExternalTableElement table)
            {
                data[element.DataPath] = new TableDataValue
                {
                    TableElementId = table.Id,
                    Rows = table.CellData ?? new List<List<string>>()
                };
            }
            else
            {
                data[element.DataPath] = element.DefaultValue ?? string.Empty;
            }
        }

        return data;
    }
}
