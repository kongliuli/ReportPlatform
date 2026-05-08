using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using ReportDataMaker.Models;

namespace ReportDataMaker.Services
{
    /// <summary>
    /// 批量导入服务 - 生成示例xlsx、批量导入和校验
    /// </summary>
    public class BatchImportService
    {
        /// <summary>
        /// 导入结果
        /// </summary>
        public class ImportResult
        {
            public bool Success { get; set; }
            public int TotalRows { get; set; }
            public int MatchedRows { get; set; }
            public int FailedRows { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
            public List<Dictionary<string, string>> Data { get; set; } = new List<Dictionary<string, string>>();
        }

        /// <summary>
        /// 生成导入模板示例 xlsx 文件
        /// 将所有可编辑字段扁平化为单行表头
        /// </summary>
        public void GenerateSampleXlsx(string filePath, ExternalTemplateDefinition template)
        {
            var editableElements = template.Elements
                .Where(e => e.Group == ElementGroup.Editable)
                .OrderBy(e => e.Y).ThenBy(e => e.X)
                .ToList();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("导入数据");

            // 表头行
            int col = 1;
            foreach (var el in editableElements)
            {
                var header = !string.IsNullOrEmpty(el.Label) ? el.Label :
                             !string.IsNullOrEmpty(el.DataPath) ? el.DataPath : el.Id;
                var cell = ws.Cell(1, col);
                cell.Value = header;
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(37, 99, 235);
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                col++;
            }

            // 示例数据行
            int row = 2;
            foreach (var el in editableElements)
            {
                var cell = ws.Cell(row, 1 + editableElements.IndexOf(el));
                cell.Value = GetSampleValue(el);
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            }

            // 调整列宽
            ws.Columns().AdjustToContents(10, 40);

            // 添加说明 sheet
            var infoSheet = workbook.Worksheets.Add("填写说明");
            infoSheet.Cell(1, 1).Value = "导入数据填写说明";
            infoSheet.Cell(1, 1).Style.Font.Bold = true;
            infoSheet.Cell(1, 1).Style.Font.FontSize = 14;
            infoSheet.Cell(3, 1).Value = "1. 请勿修改表头行";
            infoSheet.Cell(4, 1).Value = "2. 每行数据对应一份报告单";
            infoSheet.Cell(5, 1).Value = "3. 日期格式: yyyy-MM-dd (如 2026-05-07)";
            infoSheet.Cell(6, 1).Value = "4. 可添加任意多行数据批量生成";
            infoSheet.Cell(7, 1).Value = $"5. 当前模板: {template.Name}";
            infoSheet.Cell(8, 1).Value = $"6. 可编辑字段数: {editableElements.Count}";

            infoSheet.Column(1).Width = 60;

            workbook.SaveAs(filePath);
        }

        private string GetSampleValue(ExternalElementBase element)
        {
            return element switch
            {
                ExternalTextElement t => !string.IsNullOrEmpty(t.Text) ? t.Text : $"[示例{GetSampleLabel(element)}]",
                ExternalNumberElement => "0",
                ExternalDateElement => DateTime.Now.ToString("yyyy-MM-dd"),
                ExternalDropdownElement d => d.Options?.FirstOrDefault() ?? "",
                ExternalCheckboxElement => "true",
                ExternalRadioElement => "true",
                _ => $"[{element.DataPath ?? element.Id}]"
            };
        }

        private string GetSampleLabel(ExternalElementBase element)
        {
            return !string.IsNullOrEmpty(element.Label) ? element.Label :
                   !string.IsNullOrEmpty(element.DataPath) ? element.DataPath : element.Id;
        }

        /// <summary>
        /// 从 xlsx 文件批量导入数据
        /// </summary>
        public ImportResult ImportFromXlsx(string filePath, ExternalTemplateDefinition template)
        {
            var result = new ImportResult();
            var editableElements = template.Elements
                .Where(e => e.Group == ElementGroup.Editable)
                .OrderBy(e => e.Y).ThenBy(e => e.X)
                .ToList();

            if (editableElements.Count == 0)
            {
                result.Errors.Add("模板中没有可编辑字段");
                return result;
            }

            try
            {
                using var workbook = new XLWorkbook(filePath);
                var ws = workbook.Worksheet(1); // 取第一个 sheet

                // 读取表头映射
                var headerRow = ws.Row(1);
                var colMap = new Dictionary<int, ExternalElementBase>();

                for (int c = 1; c <= headerRow.LastCellUsed()?.Address.ColumnNumber; c++)
                {
                    var header = ws.Cell(1, c).GetString().Trim();
                    if (string.IsNullOrEmpty(header)) continue;

                    var matched = editableElements.FirstOrDefault(e =>
                        (e.Label != null && e.Label.Equals(header, StringComparison.OrdinalIgnoreCase)) ||
                        (e.DataPath != null && e.DataPath.Equals(header, StringComparison.OrdinalIgnoreCase)) ||
                        (e.Id != null && e.Id.Equals(header, StringComparison.OrdinalIgnoreCase)));

                    if (matched != null)
                        colMap[c] = matched;
                }

                // 读取数据行（从第2行开始）
                var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
                for (int r = 2; r <= lastRow; r++)
                {
                    var rowDict = new Dictionary<string, string>();
                    bool hasData = false;

                    foreach (var kvp in colMap)
                    {
                        var cellValue = ws.Cell(r, kvp.Key).GetString().Trim();
                        if (!string.IsNullOrEmpty(cellValue))
                            hasData = true;

                        var key = kvp.Value.DataPath ?? kvp.Value.Id;
                        rowDict[key] = cellValue;
                    }

                    if (hasData)
                    {
                        result.Data.Add(rowDict);
                        result.MatchedRows++;
                    }
                }

                result.TotalRows = lastRow - 1;
                result.Success = result.MatchedRows > 0;

                if (result.MatchedRows == 0)
                    result.Errors.Add("未能匹配任何数据行，请检查表头是否与模板字段对应");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"读取文件失败: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// 将导入数据应用到模板元素
        /// </summary>
        public int ApplyDataToTemplate(List<Dictionary<string, string>> data, ExternalTemplateDefinition template, int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= data.Count) return 0;

            var rowData = data[rowIndex];
            var editableElements = template.Elements
                .Where(e => e.Group == ElementGroup.Editable)
                .ToList();

            int applied = 0;
            foreach (var element in editableElements)
            {
                var key = element.DataPath ?? element.Id;
                if (rowData.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value))
                {
                    ApplyValue(element, value);
                    applied++;
                }
            }

            return applied;
        }

        private void ApplyValue(ExternalElementBase element, string value)
        {
            switch (element)
            {
                case ExternalTextElement t:
                    t.Text = value;
                    break;
                case ExternalNumberElement n:
                    if (double.TryParse(value, out var dv)) n.Value = dv;
                    break;
                case ExternalDateElement d:
                    d.Value = value;
                    break;
                case ExternalDropdownElement dd:
                    dd.Value = value;
                    break;
                case ExternalCheckboxElement cb:
                    cb.Checked = value.ToLower() == "true" || value == "1" || value.ToLower() == "yes";
                    break;
                case ExternalRadioElement r:
                    r.Checked = value.ToLower() == "true" || value == "1";
                    break;
            }
        }
    }
}
