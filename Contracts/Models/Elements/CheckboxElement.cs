namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>
/// 复选框元素
/// </summary>
public class CheckboxElement : ExternalElementBase
{
    public bool Checked { get; set; }
    
    public string? CheckColor { get; set; }
    
    public string? UncheckedValue { get; set; }
    
    public string? CheckedValue { get; set; }
}
