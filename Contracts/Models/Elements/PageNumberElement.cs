namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>
/// 页码元素
/// </summary>
public class PageNumberElement : ElementBase
{
    public string? Format { get; set; }
    
    public int StartPage { get; set; } = 1;
    
    public string? Align { get; set; }
}
