using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Services.ExcelAdapter;

/// <summary>扁平化字段，扩展字段模式增加元素标识</summary>
public class FlatField : FieldSchema
{
    /// <summary>关联的元素标识</summary>
    public string? ElementId { get; set; }
}

/// <summary>模板字段模式，描述模板的所有可编辑字段结构</summary>
public class TemplateFieldSchema
{
    /// <summary>模板名称</summary>
    public string TemplateName { get; set; } = string.Empty;
    /// <summary>模板版本</summary>
    public string TemplateVersion { get; set; } = string.Empty;
    /// <summary>扁平化字段列表</summary>
    public List<FlatField> Fields { get; set; } = new();
    /// <summary>生成时间</summary>
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}
