using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Registry;

namespace Xinglin.ReportEditor.Contracts.Models.Elements;

[ElementAdaptationGroup(ElementAdaptationGroup.Form)]
/// <summary>数字元素</summary>
public class NumberElement : ExternalElementBase
{
    /// <summary>数字值</summary>
    public string? Value { get; set; }

    /// <summary>最小值</summary>
    public double? MinValue { get; set; }

    /// <summary>最大值</summary>
    public double? MaxValue { get; set; }

    /// <summary>小数位数</summary>
    public int DecimalPlaces { get; set; } = 2;

    /// <summary>单位</summary>
    public string? Unit { get; set; }

    /// <summary>前缀</summary>
    public string? Prefix { get; set; }

    /// <summary>后缀</summary>
    public string? Suffix { get; set; }

    public string? Format { get; set; }
}
