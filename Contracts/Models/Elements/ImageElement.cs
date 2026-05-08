namespace Xinglin.ReportEditor.Contracts.Models.Elements;

public class ImageElement : ExternalElementBase
{
    public string? Src { get; set; }
    
    public string? Fit { get; set; }
    
    public bool MaintainAspectRatio { get; set; } = true;
    
    public string? AltText { get; set; }
    
    public string? Base64Data { get; set; }
}
