using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Adapter.Excel.Services;

/// <summary>Excel数据校验器，验证批量数据是否符合模板字段模式</summary>
public class ExcelDataValidator
{
    /// <summary>校验批量数据是否符合模板字段模式</summary>
    /// <param name="schema">模板字段模式</param>
    /// <param name="batchData">批量数据</param>
    /// <returns>校验报告</returns>
    public ValidationReport Validate(TemplateFieldSchema schema, List<Dictionary<string, object>> batchData)
    {
        var report = new ValidationReport { TotalRows = batchData.Count };
        for (int rowIdx = 0; rowIdx < batchData.Count; rowIdx++)
        {
            var rowData = batchData[rowIdx];
            foreach (var field in schema.Fields)
            {
                if (!rowData.TryGetValue(field.DataPath, out var rawValue)) continue;
                var value = rawValue?.ToString() ?? "";
                if (string.IsNullOrEmpty(value)) continue;
                var error = ValidateField(field, value);
                if (error != null)
                    report.Errors.Add(new ValidationError
                    {
                        Row = rowIdx + 1,
                        DataPath = field.DataPath,
                        Label = field.Label,
                        Value = value,
                        ExpectedType = field.DataType.ToString(),
                        Message = error
                    });
            }
        }
        report.IsValid = report.Errors.Count == 0;
        return report;
    }

    private string? ValidateField(FlatField field, string value)
    {
        return field.DataType switch
        {
            FieldDataType.Number => ValidateNumber(field, value),
            FieldDataType.Date => ValidateDate(field, value),
            FieldDataType.Dropdown => ValidateDropdown(field, value),
            FieldDataType.Boolean => ValidateBoolean(value),
            _ => null
        };
    }

    private string? ValidateNumber(FlatField field, string value)
    {
        if (!double.TryParse(value, out var num))
            return $"期望数字，实际值 \"{value}\"";
        if (field.MinValue.HasValue && num < field.MinValue.Value)
            return $"值 {num} 小于最小值 {field.MinValue}";
        if (field.MaxValue.HasValue && num > field.MaxValue.Value)
            return $"值 {num} 大于最大值 {field.MaxValue}";
        if (field.DecimalPlaces.HasValue)
        {
            var decimals = value.Contains('.') ? value.Split('.')[1].Length : 0;
            if (decimals > field.DecimalPlaces.Value)
                return $"小数位数 {decimals} 超过限制 {field.DecimalPlaces}";
        }
        return null;
    }

    private string? ValidateDate(FlatField field, string value)
    {
        var format = field.Format ?? "yyyy-MM-dd";
        if (!DateTime.TryParseExact(value, format, null, System.Globalization.DateTimeStyles.None, out _))
            if (!DateTime.TryParse(value, out _))
                return $"日期格式不匹配，期望 {format}";
        return null;
    }

    private string? ValidateDropdown(FlatField field, string value)
    {
        if (field.Options?.Count > 0 && !field.Options.Contains(value))
            return $"值 \"{value}\" 不在选项列表中 [{string.Join(",", field.Options)}]";
        return null;
    }

    private string? ValidateBoolean(string value)
    {
        var lower = value.ToLower();
        if (lower != "true" && lower != "false" && lower != "1" && lower != "0" && lower != "yes" && lower != "no")
            return $"期望布尔值 (true/false)，实际值 \"{value}\"";
        return null;
    }
}

/// <summary>校验报告，包含校验结果和错误列表</summary>
public class ValidationReport
{
    /// <summary>校验是否通过</summary>
    public bool IsValid { get; set; }
    /// <summary>总行数</summary>
    public int TotalRows { get; set; }
    /// <summary>校验错误列表</summary>
    public List<ValidationError> Errors { get; set; } = new();
    /// <summary>错误数量</summary>
    public int ErrorCount => Errors.Count;
}

/// <summary>校验错误，描述单条数据的校验错误信息</summary>
public class ValidationError
{
    /// <summary>行号</summary>
    public int Row { get; set; }
    /// <summary>数据路径</summary>
    public string DataPath { get; set; } = string.Empty;
    /// <summary>字段标签</summary>
    public string Label { get; set; } = string.Empty;
    /// <summary>实际值</summary>
    public string Value { get; set; } = string.Empty;
    /// <summary>期望类型</summary>
    public string ExpectedType { get; set; } = string.Empty;
    /// <summary>错误消息</summary>
    public string Message { get; set; } = string.Empty;
}
