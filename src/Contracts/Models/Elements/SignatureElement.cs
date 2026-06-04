using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Registry;

namespace Xinglin.ReportEditor.Contracts.Models.Elements;

[ElementAdaptationGroup(ElementAdaptationGroup.Advanced)]
/// <summary>签名元素</summary>
public class SignatureElement : ExternalElementBase
{
    /// <summary>占位提示文本</summary>
    public string? Placeholder { get; set; }

    /// <summary>签名线条颜色</summary>
    public string? LineColor { get; set; }

    /// <summary>签名线条宽度</summary>
    public double LineWidth { get; set; } = 1;

    /// <summary>签名数据</summary>
    public string? SignatureData { get; set; }

    /// <summary>是否必填</summary>
    public bool Required { get; set; }
}
