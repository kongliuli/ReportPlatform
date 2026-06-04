namespace ReportDataMaker.Models;

/// <summary>外部模板定义，表示一个完整的报告模板结构</summary>
public class ExternalTemplateDefinition
{
    /// <summary>模板唯一标识</summary>
    public string? Id { get; set; }
    /// <summary>模板名称</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>模板类型</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>模板版本号</summary>
    public string Version { get; set; } = "1";
    /// <summary>医院标识</summary>
    public string? HospitalId { get; set; }
    /// <summary>页面宽度（毫米）</summary>
    public double PageWidth { get; set; } = 210;
    /// <summary>页面高度（毫米）</summary>
    public double PageHeight { get; set; } = 297;
    /// <summary>页面方向</summary>
    public string Orientation { get; set; } = "Portrait";
    /// <summary>左边距（毫米）</summary>
    public double MarginLeft { get; set; } = 20;
    /// <summary>右边距（毫米）</summary>
    public double MarginRight { get; set; } = 20;
    /// <summary>上边距（毫米）</summary>
    public double MarginTop { get; set; } = 20;
    /// <summary>下边距（毫米）</summary>
    public double MarginBottom { get; set; } = 20;
    /// <summary>背景颜色</summary>
    public string BackgroundColor { get; set; } = "#FFFFFF";
    /// <summary>全局字体大小</summary>
    public double GlobalFontSize { get; set; }
    /// <summary>是否启用全局字体大小</summary>
    public bool EnableGlobalFontSize { get; set; }
    /// <summary>模板元素列表</summary>
    public List<ReportExternalElementBase> Elements { get; set; } = new();
    /// <summary>数据绑定定义列表</summary>
    public List<LegacyDataBindingDefinition> DataBindings { get; set; } = new();
    /// <summary>模板文件路径</summary>
    public string? FilePath { get; set; }
}

/// <summary>遗留数据绑定定义，描述元素与数据路径的绑定关系</summary>
public class LegacyDataBindingDefinition
{
    /// <summary>绑定定义唯一标识</summary>
    public string? Id { get; set; }
    /// <summary>关联的元素标识</summary>
    public string ElementId { get; set; } = string.Empty;
    /// <summary>数据路径</summary>
    public string DataPath { get; set; } = string.Empty;
    /// <summary>绑定类型</summary>
    public string BindingType { get; set; } = "Text";
    /// <summary>格式化字符串</summary>
    public string? FormatString { get; set; }
    /// <summary>默认值</summary>
    public string? DefaultValue { get; set; }
}
