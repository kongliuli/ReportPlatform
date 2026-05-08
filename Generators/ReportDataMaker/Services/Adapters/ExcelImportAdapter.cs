using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using ReportDataMaker.Models;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;
using IDataAdapter = ReportDataMaker.Models.IDataAdapter;

namespace ReportDataMaker.Services.Adapters
{
    public class ExcelImportAdapter : IDataAdapter
    {
        private readonly List<string> _targetPaths = new List<string>();

        public string AdapterId => "excel-import";
        public string AdapterName => "ExcelImportAdapter";
        public AdapterType Type => AdapterType.Excel;
        public IReadOnlyList<string> TargetDataPaths => _targetPaths.AsReadOnly();

        public ExcelImportAdapter()
        {
        }

        public void SetTargetPaths(IEnumerable<string> paths)
        {
            _targetPaths.Clear();
            _targetPaths.AddRange(paths);
        }

        public Dictionary<string, object> ReadData(IReadOnlyDictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue("filePath", out var filePathObj) || !(filePathObj is string filePath))
                throw new ArgumentException("Parameters must contain 'filePath' key with the Excel file path.");

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Excel file not found: {filePath}");

            var result = new Dictionary<string, object>();
            var missingColumns = new List<string>();

            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet(1);
            var headerRow = worksheet.Row(1);

            var columnMap = new Dictionary<int, string>();
            var maxCol = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;

            for (int col = 1; col <= maxCol; col++)
            {
                var headerText = headerRow.Cell(col).GetString().Trim();
                if (!string.IsNullOrEmpty(headerText))
                {
                    columnMap[col] = headerText;
                }
            }

            var dataRow = worksheet.Row(2);
            foreach (var kvp in columnMap)
            {
                var colIndex = kvp.Key;
                var dataPath = kvp.Value;

                if (_targetPaths.Contains(dataPath))
                {
                    var cell = dataRow.Cell(colIndex);
                    var rawValue = cell.GetString();
                    if (!string.IsNullOrEmpty(rawValue))
                    {
                        result[dataPath] = rawValue;
                    }
                }
            }

            missingColumns = _targetPaths.Where(p => !columnMap.Values.Contains(p)).ToList();

            if (missingColumns.Count > 0)
            {
                result["_missingColumns"] = string.Join(", ", missingColumns);
            }

            return result;
        }

        public Task<AdapterResult> ReadDataAsync()
        {
            var data = ReadData(new Dictionary<string, object>());
            return Task.FromResult(new AdapterResult { Success = true, Data = data });
        }

        public Task<AdapterResult> ReadBatchDataAsync()
        {
            return Task.FromResult(new AdapterResult { Success = true });
        }

        public Task<ValidationResult> ValidateConfigAsync()
        {
            return Task.FromResult(ValidationResult.Success);
        }

        public void ExportTemplate(string filePath, IEnumerable<string> dataPaths)
        {
            var paths = dataPaths.ToList();
            if (paths.Count == 0)
                return;

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("数据导入模板");

            for (int i = 0; i < paths.Count; i++)
            {
                worksheet.Cell(1, i + 1).Value = paths[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(filePath);
        }

        public ImportResult ImportFromFile(string filePath)
        {
            var result = new ImportResult();

            try
            {
                using var workbook = new XLWorkbook(filePath);
                var worksheet = workbook.Worksheet(1);

                var headerRow = worksheet.Row(1);
                var maxCol = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;
                var columnMap = new Dictionary<int, string>();

                for (int col = 1; col <= maxCol; col++)
                {
                    var headerText = headerRow.Cell(col).GetString().Trim();
                    if (!string.IsNullOrEmpty(headerText))
                        columnMap[col] = headerText;
                }

                var unrecognizedColumns = columnMap.Values
                    .Where(h => !_targetPaths.Contains(h)).ToList();

                var maxRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

                for (int row = 2; row <= maxRow; row++)
                {
                    var rowData = new Dictionary<string, object>();
                    var dataRow = worksheet.Row(row);

                    for (int col = 1; col <= maxCol; col++)
                    {
                        if (columnMap.TryGetValue(col, out var dataPath) && _targetPaths.Contains(dataPath))
                        {
                            var rawValue = dataRow.Cell(col).GetString();
                            if (!string.IsNullOrEmpty(rawValue))
                                rowData[dataPath] = rawValue;
                        }
                    }

                    if (rowData.Count > 0)
                        result.Rows.Add(rowData);
                }

                var foundColumns = columnMap.Values.Intersect(_targetPaths).ToList();
                var missingColumns = _targetPaths.Except(foundColumns).ToList();

                result.UnrecognizedColumns = unrecognizedColumns;
                result.MissingColumns = missingColumns;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }
    }

    public class ImportResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public List<string> UnrecognizedColumns { get; set; } = new List<string>();
        public List<string> MissingColumns { get; set; } = new List<string>();
        public List<Dictionary<string, object>> Rows { get; set; } = new List<Dictionary<string, object>>();
    }
}
