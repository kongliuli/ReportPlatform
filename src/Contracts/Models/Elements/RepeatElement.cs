using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Registry;

namespace Xinglin.ReportEditor.Contracts.Models.Elements;

[ElementAdaptationGroup(ElementAdaptationGroup.Data)]
/// <summary>重复元素</summary>
public class RepeatElement : ExternalElementBase
{
    /// <summary>数据源路径</summary>
    public string? DataSource { get; set; }

    /// <summary>项模板</summary>
    public string? ItemTemplate { get; set; }

    /// <summary>重复方向</summary>
    public string? Direction { get; set; }

    /// <summary>间距</summary>
    public double Gap { get; set; }

    /// <summary>分隔符</summary>
    public string? Separator { get; set; }

    /// <summary>最大重复次数</summary>
    public int? MaxCount { get; set; }
}
