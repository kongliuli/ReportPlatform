namespace Xinglin.ReportEditor.Contracts.Models.Elements;

public class SignatureElement : ExternalElementBase
{
    public string? Placeholder { get; set; }
    
    public string? LineColor { get; set; }
    
    public double LineWidth { get; set; } = 1;
    
    public string? SignatureData { get; set; }
    
    public bool Required { get; set; }
}
