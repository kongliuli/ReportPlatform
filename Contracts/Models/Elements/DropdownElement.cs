namespace Xinglin.ReportEditor.Contracts.Models.Elements;

/// <summary>下拉选择元素</summary>
public class DropdownElement : ExternalElementBase
{
    /// <summary>选项列表</summary>
    public List<string> Options { get; set; } = new();

    /// <summary>已选中的值</summary>
    public string? SelectedValue { get; set; }

    /// <summary>是否多选</summary>
    public bool IsMultiSelect { get; set; }

    /// <summary>多选时已选中的值列表</summary>
    public List<string>? SelectedValues { get; set; }

    /// <summary>是否允许自定义输入</summary>
    public bool AllowCustom { get; set; }

    /// <summary>是否可搜索</summary>
    public bool Searchable { get; set; }
}
