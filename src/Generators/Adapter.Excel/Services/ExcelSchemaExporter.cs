using ClosedXML.Excel;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Adapter.Excel.Services;

public class ExcelSchemaExporter
{
    private static readonly XLColor TableGroupColor = XLColor.FromArgb(16, 185, 129);
    private static readonly XLColor NormalHeaderColor = XLColor.FromArgb(37, 99, 235);

    public void ExportTemplate(string filePath, TemplateFieldSchema schema)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("数据导入");

        for (int i = 0; i < schema.Fields.Count; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = schema.Fields[i].DataPath;
            cell.Style.Font.FontColor = XLColor.LightGray;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(245, 245, 245);
        }
        ws.Row(1).Hide();

        for (int i = 0; i < schema.Fields.Count; i++)
        {
            var field = schema.Fields[i];
            var cell = ws.Cell(2, i + 1);
            cell.Value = field.Label;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = field.TableLabel != null ? TableGroupColor : NormalHeaderColor;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            if (field.IsRequired)
                cell.Style.Font.Bold = true;
        }

        for (int i = 0; i < schema.Fields.Count; i++)
        {
            var field = schema.Fields[i];
            var cell = ws.Cell(3, i + 1);
            cell.Value = BuildTypeRowValue(field);
            cell.Style.Font.FontColor = XLColor.LightGray;
        }
        ws.Row(3).Hide();

        for (int i = 0; i < schema.Fields.Count; i++)
        {
            var cell = ws.Cell(4, i + 1);
            cell.Value = GetExampleValue(schema.Fields[i]);
            cell.Style.Font.FontColor = XLColor.Gray;
            cell.Style.Font.Italic = true;
        }

        ApplyTableGroupBorders(ws, schema);

        AddInstructionSheet(workbook, schema);
        ws.Columns().AdjustToContents(10, 40);
        workbook.SaveAs(filePath);
    }

    private static string BuildTypeRowValue(FlatField field)
    {
        var value = field.DataType.ToString().ToLower();
        if (field.DataType == FieldDataType.Dropdown && field.Options?.Count > 0)
            value += $" [{string.Join(",", field.Options)}]";
        if (field.DataType == FieldDataType.Number && field.DecimalPlaces.HasValue)
            value += $" (D{field.DecimalPlaces})";
        return value;
    }

    private string GetExampleValue(FlatField field)
    {
        return field.DataType switch
        {
            FieldDataType.Number => "0",
            FieldDataType.Date => DateTime.Now.ToString(field.Format ?? "yyyy-MM-dd"),
            FieldDataType.Dropdown => field.Options?.FirstOrDefault() ?? "",
            FieldDataType.Boolean => "true/false",
            _ => $"[输入{field.Label}]"
        };
    }

    private static void ApplyTableGroupBorders(IXLWorksheet ws, TemplateFieldSchema schema)
    {
        string? currentTableLabel = null;
        int groupStartCol = -1;

        for (int i = 0; i <= schema.Fields.Count; i++)
        {
            var fieldLabel = i < schema.Fields.Count ? schema.Fields[i].TableLabel : null;

            if (fieldLabel != currentTableLabel)
            {
                if (currentTableLabel != null && groupStartCol >= 0)
                {
                    var range = ws.Range(2, groupStartCol, 2, i);
                    range.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                    range.Style.Border.OutsideBorderColor = XLColor.FromArgb(5, 150, 105);
                }

                currentTableLabel = fieldLabel;
                groupStartCol = fieldLabel != null ? i + 1 : -1;
            }
        }
    }

    private void AddInstructionSheet(XLWorkbook workbook, TemplateFieldSchema schema)
    {
        var ws = workbook.Worksheets.Add("填写说明");
        ws.Cell(1, 1).Value = "数据导入填写说明";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Cell(3, 1).Value = "1. 请勿修改第1行（隐藏的契约行）";
        ws.Cell(4, 1).Value = "2. 请勿修改第2行（列头行）";
        ws.Cell(5, 1).Value = "3. 从第4行开始填写数据";
        ws.Cell(6, 1).Value = "4. 日期格式: yyyy-MM-dd";
        ws.Cell(7, 1).Value = "5. 布尔值: true/false";
        ws.Cell(8, 1).Value = $"6. 模板: {schema.TemplateName} v{schema.TemplateVersion}";
        ws.Cell(9, 1).Value = $"7. 可编辑字段数: {schema.Fields.Count}";

        var tableCount = schema.Fields.Where(f => f.TableLabel != null)
            .Select(f => f.TableLabel).Distinct().Count();
        if (tableCount > 0)
            ws.Cell(10, 1).Value = $"8. 含 {tableCount} 个表格的可编辑单元格（绿色表头标识）";
        ws.Cell(11, 1).Value = "9. 每行数据对应一份报告单（批量模式）";
        ws.Column(1).Width = 60;
    }
}
