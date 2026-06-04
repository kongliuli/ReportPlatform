using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Registry;

namespace Xinglin.ReportEditor.Contracts.Models.Elements;

[ElementAdaptationGroup(ElementAdaptationGroup.Advanced)]
/// <summary>二维码元素</summary>
public class QrCodeElement : ExternalElementBase
{
    /// <summary>二维码值</summary>
    public string? Value { get; set; }

    /// <summary>纠错级别</summary>
    public string? ErrorCorrectionLevel { get; set; }

    /// <summary>边距</summary>
    public int Margin { get; set; } = 4;

    /// <summary>二维码颜色</summary>
    public string? Color { get; set; }

    /// <summary>二维码尺寸</summary>
    public int Size { get; set; } = 100;
}
