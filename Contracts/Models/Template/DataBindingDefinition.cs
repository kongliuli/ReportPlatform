namespace Xinglin.ReportEditor.Contracts.Models.Template;

/// <summary>
/// 数据绑定定义
/// </summary>
public class DataBindingDefinition
{
    public string? Id { get; set; }
    
    public string ElementId { get; set; } = string.Empty;
    
    public string DataPath { get; set; } = string.Empty;
    
    public BindingType BindingType { get; set; } = BindingType.Text;
    
    public string? FormatString { get; set; }
    
    public string? DefaultValue { get; set; }
    
    public string? Transform { get; set; }
}

/// <summary>
/// 绑定类型
/// </summary>
public enum BindingType
{
    Text,
    Image,
    Visibility,
    Repeat,
    Style
}
