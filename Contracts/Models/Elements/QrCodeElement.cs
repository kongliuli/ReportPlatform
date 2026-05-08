namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>
/// 二维码元素
/// </summary>
public class QrCodeElement : ExternalElementBase
{
    public string? Value { get; set; }
    
    public string? ErrorCorrectionLevel { get; set; }
    
    public int Margin { get; set; } = 4;
    
    public string? Color { get; set; }
    
    public int Size { get; set; } = 100;
}
