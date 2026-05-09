namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>条形码元素</summary>
public class BarcodeElement : ExternalElementBase
{
    /// <summary>条形码值</summary>
    public string? Value { get; set; }

    /// <summary>条形码格式</summary>
    public string? Format { get; set; }

    /// <summary>是否显示文本</summary>
    public bool ShowText { get; set; } = true;

    /// <summary>线条颜色</summary>
    public string? LineColor { get; set; }
}
