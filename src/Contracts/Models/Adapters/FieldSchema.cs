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
    Boolean,

    // ===== 表格填值抽象 =====

    /// <summary>表格类型</summary>
    Table,

    // ===== 适配预留桩（暂不实现完整填值UI）=====

    /// <summary>图片类型（预留）</summary>
    Image,

    /// <summary>图表类型（预留）</summary>
    Chart,

    /// <summary>条形码类型（预留）</summary>
    Barcode,

    /// <summary>二维码类型（预留）</summary>
    QrCode,

    /// <summary>签名类型（预留）</summary>
    Signature,

    /// <summary>超链接类型（预留）</summary>
    Hyperlink,

    /// <summary>只读/预览类型</summary>
    ReadOnly,

    /// <summary>列表/重复项类型（预留）</summary>
    List
}

/// <summary>表格列模式定义（描述表格中每列的结构）</summary>
public class TableColumnSchema
{
    public int ColumnIndex { get; set; }
    public string? Header { get; set; }
    public FieldDataType CellDataType { get; set; } = FieldDataType.Text;
    public string? DataPath { get; set; }
    public string? Format { get; set; }
    public List<string>? Options { get; set; }
}

/// <summary>字段模式定义</summary>
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

    /// <summary>表格列模式（仅 DataType=Table 时有效）</summary>
    public List<TableColumnSchema>? TableColumns { get; set; }

    /// <summary>表格行数（仅 DataType=Table 时有效）</summary>
    public int? TableRows { get; set; }

    /// <summary>表头行数（仅 DataType=Table 时有效）</summary>
    public int? TableHeaderRows { get; set; }
}
