namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>页码元素</summary>
public class PageNumberElement : ElementBase
{
    /// <summary>页码格式</summary>
    public string? Format { get; set; }

    /// <summary>起始页码</summary>
    public int StartPage { get; set; } = 1;

    /// <summary>对齐方式</summary>
    public string? Align { get; set; }
}
