namespace Xinglin.ReportEditor.Contracts.Models.Adapters;

/// <summary>
/// 字段数据类型
/// </summary>
public enum FieldDataType
{
    Text,
    Number,
    Date,
    Dropdown,
    Boolean
}

/// <summary>
/// 字段模式定义
/// </summary>
public class FieldSchema
{
    public string DataPath { get; set; } = string.Empty;
    
    public string Label { get; set; } = string.Empty;
    
    public FieldDataType DataType { get; set; } = FieldDataType.Text;
    
    public string? Format { get; set; }
    
    public List<string>? Options { get; set; }
    
    public bool IsRequired { get; set; }
    
    public double? MinValue { get; set; }
    
    public double? MaxValue { get; set; }
    
    public int? DecimalPlaces { get; set; }
}
