using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Services.ExcelAdapter;

public class ExcelDataValidator
{
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

public class ValidationReport
{
    public bool IsValid { get; set; }
    public int TotalRows { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
    public int ErrorCount => Errors.Count;
}

public class ValidationError
{
    public int Row { get; set; }
    public string DataPath { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ExpectedType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
