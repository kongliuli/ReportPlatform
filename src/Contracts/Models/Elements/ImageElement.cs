using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Registry;

namespace Xinglin.ReportEditor.Contracts.Models.Elements;

[ElementAdaptationGroup(ElementAdaptationGroup.Advanced)]
/// <summary>图片元素</summary>
public class ImageElement : ExternalElementBase
{
    /// <summary>图片源地址</summary>
    public string? Src { get; set; }

    /// <summary>图片适应模式</summary>
    public string? Fit { get; set; }

    /// <summary>是否保持宽高比</summary>
    public bool MaintainAspectRatio { get; set; } = true;

    /// <summary>替代文本</summary>
    public string? AltText { get; set; }

    /// <summary>Base64编码的图片数据</summary>
    public string? Base64Data { get; set; }
}
