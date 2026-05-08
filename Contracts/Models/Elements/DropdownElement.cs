namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>
/// 下拉选择元素
/// </summary>
public class DropdownElement : ExternalElementBase
{
    public List<string> Options { get; set; } = new();
    
    public string? SelectedValue { get; set; }
    
    public bool IsMultiSelect { get; set; }
    
    public List<string>? SelectedValues { get; set; }
    
    public bool AllowCustom { get; set; }
    
    public bool Searchable { get; set; }
}
