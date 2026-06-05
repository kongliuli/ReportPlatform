using ClosedXML.Excel;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Adapter.Excel.Services;

public class ExcelContractReader
{
    public AdapterResult ReadByContract(string filePath, ExcelTemplateSchema schema)
    {
        var result = new AdapterResult { Success = true };
        try
        {
            using var workbook = new XLWorkbook(filePath);
            var ws = workbook.Worksheet(1);
            var contractMap = ReadContractRow(ws, schema);
            if (contractMap.Count == 0)
                return Fail("未找到契约行，请确认使用正确的模板文件");

            var dataRow = ws.Row(schema.DataStartRow);
            foreach (var kvp in contractMap)
            {
                var cellValue = dataRow.Cell(kvp.Key).GetString().Trim();
                if (!string.IsNullOrEmpty(cellValue))
                    result.Data[kvp.Value] = cellValue;
            }
        }
        catch (Exception ex)
        {
            return Fail($"读取文件失败: {ex.Message}");
        }
        return result;
    }

    public AdapterResult ReadBatchByContract(string filePath, ExcelTemplateSchema schema)
    {
        var result = new AdapterResult { Success = true };
        try
        {
            using var workbook = new XLWorkbook(filePath);
            var ws = workbook.Worksheet(1);
            var contractMap = ReadContractRow(ws, schema);
            if (contractMap.Count == 0)
                return Fail("未找到契约行");

            var lastRow = ws.LastRowUsed()?.RowNumber() ?? schema.DataStartRow;
            for (int row = schema.DataStartRow; row <= lastRow; row++)
            {
                var rowData = new Dictionary<string, object>();
                bool hasData = false;
                foreach (var kvp in contractMap)
                {
                    var cellValue = ws.Cell(row, kvp.Key).GetString().Trim();
                    if (!string.IsNullOrEmpty(cellValue)) hasData = true;
                    rowData[kvp.Value] = cellValue;
                }
                if (hasData)
                    result.BatchData.Add(rowData);
            }
        }
        catch (Exception ex)
        {
            return Fail($"批量读取失败: {ex.Message}");
        }
        return result;
    }

    private Dictionary<int, string> ReadContractRow(IXLWorksheet ws, ExcelTemplateSchema schema)
    {
        var map = new Dictionary<int, string>();
        var contractRow = ws.Row(schema.ContractRow);
        var maxCol = ws.Row(schema.LabelRow).LastCellUsed()?.Address.ColumnNumber ?? 0;
        for (int col = 1; col <= maxCol; col++)
        {
            var dataPath = contractRow.Cell(col).GetString().Trim();
            if (!string.IsNullOrEmpty(dataPath))
                map[col] = dataPath;
        }
        return map;
    }

    private AdapterResult Fail(string message) => new() { Success = false, ErrorMessage = message };
}
