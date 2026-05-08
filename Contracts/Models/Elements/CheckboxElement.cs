namespace Xinglin.ReportEditor.Contracts.Models.Elements;

public class CheckboxElement : ExternalElementBase
{
    public bool Checked { get; set; }
    
    public string? CheckColor { get; set; }
    
    public string? UncheckedValue { get; set; }
    
    public string? CheckedValue { get; set; }
}
