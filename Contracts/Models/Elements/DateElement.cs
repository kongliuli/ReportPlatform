namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>
/// 日期输入元素
/// </summary>
public class DateElement : ExternalElementBase
{
    public string? Value { get; set; }
    
    public string Format { get; set; } = "yyyy-MM-dd";
    
    public string? MinDate { get; set; }
    
    public string? MaxDate { get; set; }
    
    public bool ShowTime { get; set; }
    
    public string TimeFormat { get; set; } = "HH:mm";
}
