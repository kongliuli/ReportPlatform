using Xinglin.ReportEditor.Contracts.Enums;

namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>外部元素基类，支持数据绑定和适配器的元素</summary>
public abstract class ExternalElementBase : ElementBase
{
    /// <summary>元素标签</summary>
    public string? Label { get; set; }

    /// <summary>数据绑定路径</summary>
    public string? DataPath { get; set; }

    /// <summary>是否已绑定数据</summary>
    public bool IsDataBound => !string.IsNullOrEmpty(DataPath);

    /// <summary>是否为必填项</summary>
    public bool IsRequired { get; set; }

    /// <summary>元素所属分组</summary>
    public ElementGroup Group { get; set; } = ElementGroup.Fixed;

    /// <summary>关联的数据适配器标识</summary>
    public string? AdapterId { get; set; }
}
