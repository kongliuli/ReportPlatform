using ClosedXML.Excel;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Services.ExcelAdapter;

/// <summary>Excel模式导出器，将模板字段模式导出为Excel文件</summary>
public class ExcelSchemaExporter
{
    /// <summary>将模板字段模式导出为Excel文件</summary>
    /// <param name="filePath">导出文件路径</param>
    /// <param name="schema">模板字段模式</param>
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
            var cell = ws.Cell(2, i + 1);
            cell.Value = schema.Fields[i].Label;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(37, 99, 235);
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            if (schema.Fields[i].IsRequired)
                cell.Style.Font.Bold = true;
        }

        for (int i = 0; i < schema.Fields.Count; i++)
        {
            var cell = ws.Cell(3, i + 1);
            cell.Value = schema.Fields[i].DataType.ToString().ToLower();
            cell.Style.Font.FontColor = XLColor.LightGray;
            if (schema.Fields[i].DataType == FieldDataType.Dropdown && schema.Fields[i].Options?.Count > 0)
                cell.Value += $" [{string.Join(",", schema.Fields[i].Options)}]";
            if (schema.Fields[i].DataType == FieldDataType.Number && schema.Fields[i].DecimalPlaces.HasValue)
                cell.Value += $" (D{schema.Fields[i].DecimalPlaces})";
        }
        ws.Row(3).Hide();

        for (int i = 0; i < schema.Fields.Count; i++)
        {
            var cell = ws.Cell(4, i + 1);
            cell.Value = GetExampleValue(schema.Fields[i]);
            cell.Style.Font.FontColor = XLColor.Gray;
            cell.Style.Font.Italic = true;
        }

        AddInstructionSheet(workbook, schema);
        ws.Columns().AdjustToContents(10, 40);
        workbook.SaveAs(filePath);
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
        ws.Cell(10, 1).Value = "8. 每行数据对应一份报告单（批量模式）";
        ws.Column(1).Width = 60;
    }
}
