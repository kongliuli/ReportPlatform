namespace Xinglin.ReportEditor.Contracts.Models.Elements;

public class RadioElement : ExternalElementBase
{
    public string? GroupName { get; set; }
    
    public string? Value { get; set; }
    
    public bool IsChecked { get; set; }
}
