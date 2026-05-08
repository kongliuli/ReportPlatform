namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>日期元素</summary>
public class DateElement : ExternalElementBase
{
    /// <summary>日期值</summary>
    public string? Value { get; set; }

    /// <summary>日期格式</summary>
    public string Format { get; set; } = "yyyy-MM-dd";

    /// <summary>最小日期</summary>
    public string? MinDate { get; set; }

    /// <summary>最大日期</summary>
    public string? MaxDate { get; set; }

    /// <summary>是否显示时间</summary>
    public bool ShowTime { get; set; }

    /// <summary>时间格式</summary>
    public string TimeFormat { get; set; } = "HH:mm";
}
