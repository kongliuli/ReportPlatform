namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>
/// 条码元素
/// </summary>
public class BarcodeElement : ExternalElementBase
{
    public string? Value { get; set; }
    
    public string? Format { get; set; }
    
    public bool ShowText { get; set; } = true;
    
    public string? LineColor { get; set; }
    
    public int Height { get; set; } = 50;
}
