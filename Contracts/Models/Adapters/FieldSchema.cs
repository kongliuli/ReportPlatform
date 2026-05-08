namespace Xinglin.ReportEditor.Contracts.Models.Adapters;

/// <summary>字段数据类型枚举</summary>
public enum FieldDataType
{
    /// <summary>文本类型</summary>
    Text,

    /// <summary>数字类型</summary>
    Number,

    /// <summary>日期类型</summary>
    Date,

    /// <summary>下拉选择类型</summary>
    Dropdown,

    /// <summary>布尔类型</summary>
    Boolean
}

/// <summary>字段模式定义</summary>
public class FieldSchema
{
    /// <summary>字段数据路径</summary>
    public string DataPath { get; set; } = string.Empty;

    /// <summary>字段标签</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>字段数据类型</summary>
    public FieldDataType DataType { get; set; } = FieldDataType.Text;

    /// <summary>字段格式</summary>
    public string? Format { get; set; }

    /// <summary>选项列表</summary>
    public List<string>? Options { get; set; }

    /// <summary>是否必填</summary>
    public bool IsRequired { get; set; }

    /// <summary>最小值</summary>
    public double? MinValue { get; set; }

    /// <summary>最大值</summary>
    public double? MaxValue { get; set; }

    /// <summary>小数位数</summary>
    public int? DecimalPlaces { get; set; }
}
