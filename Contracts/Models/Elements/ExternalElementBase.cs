using Xinglin.ReportEditor.Contracts.Enums;

namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>
/// 外部元素基类，支持数据绑定和适配分组
/// </summary>
public abstract class ExternalElementBase : ElementBase
{
    /// <summary>
    /// 元素标签/标题
    /// </summary>
    public string? Label { get; set; }
    
    /// <summary>
    /// 数据路径，如 "Patient.Name"、"Context.Hospital"
    /// </summary>
    public string? DataPath { get; set; }
    
    /// <summary>
    /// 是否已绑定数据
    /// </summary>
    public bool IsDataBound => !string.IsNullOrEmpty(DataPath);
    
    /// <summary>
    /// 是否必填
    /// </summary>
    public bool IsRequired { get; set; }
    
    /// <summary>
    /// 适配分组
    /// </summary>
    public ElementGroup Group { get; set; } = ElementGroup.Fixed;
    
    /// <summary>
    /// 关联的适配器 ID
    /// </summary>
    public string? AdapterId { get; set; }
}
