using Xinglin.ReportEditor.Contracts.Models.Elements;

namespace Xinglin.ReportEditor.Contracts.Models.Template;

/// <summary>模板定义，表示一个完整的报表模板</summary>
public class TemplateDefinition
{
    /// <summary>模板唯一标识</summary>
    public string? Id { get; set; }

    /// <summary>模板名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>模板版本</summary>
    public int Version { get; set; } = 1;

    /// <summary>模板类型</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>所属医院标识</summary>
    public string? HospitalId { get; set; }

    /// <summary>页面设置</summary>
    public PageSettings PageSettings { get; set; } = new();

    /// <summary>数据绑定定义列表</summary>
    public List<DataBindingDefinition> DataBindings { get; set; } = new();

    /// <summary>模板元素列表</summary>
    public List<ElementBase> Elements { get; set; } = new();

    /// <summary>是否启用全局字体大小</summary>
    public bool EnableGlobalFontSize { get; set; }

    /// <summary>全局字体大小</summary>
    public double? GlobalFontSize { get; set; }
}
