namespace Xinglin.ReportEditor.Contracts.Models.Elements;

public class RepeatElement : ExternalElementBase
{
    public string? DataSource { get; set; }
    
    public string? ItemTemplate { get; set; }
    
    public string? Direction { get; set; }
    
    public double Gap { get; set; }
    
    public string? Separator { get; set; }
    
    public int? MaxCount { get; set; }
}
