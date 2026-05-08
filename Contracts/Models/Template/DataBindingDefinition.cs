namespace Xinglin.ReportEditor.Contracts.Models.Template;

/// <summary>数据绑定定义</summary>
public class DataBindingDefinition
{
    /// <summary>绑定定义唯一标识</summary>
    public string? Id { get; set; }

    /// <summary>关联的元素标识</summary>
    public string ElementId { get; set; } = string.Empty;

    /// <summary>数据路径</summary>
    public string DataPath { get; set; } = string.Empty;

    /// <summary>绑定类型</summary>
    public BindingType BindingType { get; set; } = BindingType.Text;

    /// <summary>格式化字符串</summary>
    public string? FormatString { get; set; }

    /// <summary>默认值</summary>
    public string? DefaultValue { get; set; }

    /// <summary>数据转换方式</summary>
    public string? Transform { get; set; }
}

/// <summary>绑定类型枚举</summary>
public enum BindingType
{
    /// <summary>文本绑定</summary>
    Text,

    /// <summary>图片绑定</summary>
    Image,

    /// <summary>可见性绑定</summary>
    Visibility,

    /// <summary>重复绑定</summary>
    Repeat,

    /// <summary>样式绑定</summary>
    Style
}
