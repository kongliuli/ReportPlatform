using Xinglin.ReportEditor.Contracts.Models.Adapters;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Services;

public class DataBindingService : IDataBindingService
{
    public void ApplyData(TemplateDefinition template, Dictionary<string, object> data)
    {
        if (template.Elements == null) return;

        foreach (var element in template.Elements)
        {
            if (element is TableElement table)
            {
                ApplyTableCellData(table, data);
                continue;
            }

            if (string.IsNullOrEmpty(element.DataPath) || !data.TryGetValue(element.DataPath, out var value))
                continue;

            element.DefaultValue = value?.ToString() ?? string.Empty;
        }
    }

    private void ApplyTableCellData(TableElement table, Dictionary<string, object> data)
    {
        if (table.Cells == null || table.Cells.Count == 0)
        {
            if (!string.IsNullOrEmpty(table.DataPath) && data.TryGetValue(table.DataPath, out var value))
                TryApplyTableDataWhole(table, value);
            return;
        }

        var tableDataPath = !string.IsNullOrEmpty(table.DataPath) ? table.DataPath : table.Id;
        var editableCells = table.Cells.Where(c => c.IsEditable).ToList();
        bool hasCellLevelData = editableCells.Any(c =>
            !string.IsNullOrEmpty(c.DataPath) && data.ContainsKey(c.DataPath));

        if (!hasCellLevelData)
        {
            hasCellLevelData = editableCells.Any(c =>
                data.ContainsKey($"{tableDataPath}.R{c.Row}C{c.Col}"));
        }

        if (hasCellLevelData)
        {
            EnsureCellDataInitialized(table);
            foreach (var cell in editableCells)
            {
                var cellDataPath = !string.IsNullOrEmpty(cell.DataPath)
                    ? cell.DataPath
                    : $"{tableDataPath}.R{cell.Row}C{cell.Col}";

                if (data.TryGetValue(cellDataPath, out var value))
                    table.CellData![cell.Row][cell.Col] = value?.ToString() ?? string.Empty;
            }
        }
        else if (!string.IsNullOrEmpty(table.DataPath) && data.TryGetValue(table.DataPath, out var wholeValue))
        {
            TryApplyTableDataWhole(table, wholeValue);
        }
    }

    private static void EnsureCellDataInitialized(TableElement table)
    {
        if (table.CellData != null && table.CellData.Count >= table.Rows)
        {
            foreach (var row in table.CellData)
            {
                if (row.Count < table.Cols)
                {
                    while (row.Count < table.Cols)
                        row.Add(string.Empty);
                }
            }
            return;
        }

        var newCellData = new List<List<string>>();
        for (int r = 0; r < table.Rows; r++)
        {
            var row = new List<string>();
            if (table.CellData != null && r < table.CellData.Count)
            {
                for (int c = 0; c < table.Cols; c++)
                    row.Add(c < table.CellData[r].Count ? table.CellData[r][c] : string.Empty);
            }
            else
            {
                for (int c = 0; c < table.Cols; c++)
                    row.Add(string.Empty);
            }
            newCellData.Add(row);
        }
        table.CellData = newCellData;
    }

    private static bool TryApplyTableDataWhole(TableElement table, object value)
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

    public Dictionary<string, object> ExtractData(TemplateDefinition template)
    {
        var data = new Dictionary<string, object>();
        if (template.Elements == null) return data;

        foreach (var element in template.Elements)
        {
            if (element is TableElement table)
            {
                ExtractTableData(table, data);
                continue;
            }

            if (string.IsNullOrEmpty(element.DataPath))
                continue;

            data[element.DataPath] = element.DefaultValue ?? string.Empty;
        }

        return data;
    }

    private static void ExtractTableData(TableElement table, Dictionary<string, object> data)
    {
        if (table.Cells != null)
        {
            var tableDataPath = !string.IsNullOrEmpty(table.DataPath) ? table.DataPath : table.Id;
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

                data[cellDataPath] = cellValue;
            }
        }

        if (!string.IsNullOrEmpty(table.DataPath))
        {
            data[table.DataPath] = new TableDataValue
            {
                TableElementId = table.Id,
                Rows = table.CellData ?? new List<List<string>>()
            };
        }
    }
}
