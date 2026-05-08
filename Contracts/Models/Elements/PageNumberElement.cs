namespace Xinglin.ReportEditor.Contracts.Models.Elements;

public class PageNumberElement : ElementBase
{
    public string? Format { get; set; }
    
    public int StartPage { get; set; } = 1;
    
    public string? Align { get; set; }
}
