namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>
/// 单选按钮元素
/// </summary>
public class RadioElement : ExternalElementBase
{
    public string? GroupName { get; set; }
    
    public string? Value { get; set; }
    
    public bool IsChecked { get; set; }
}
