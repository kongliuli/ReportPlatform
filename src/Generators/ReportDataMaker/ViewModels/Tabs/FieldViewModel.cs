using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.ViewModels.Tabs;

/// <summary>字段视图模型，用于数据录入和模板选择器的数据绑定</summary>
public class FieldViewModel
{
    public string ElementId { get; set; } = string.Empty;
    public string DataPath { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public FieldDataType FieldType { get; set; }
    public string Value { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsMultiLine { get; set; }

    public ObservableCollection<string> Options { get; set; } = new();
    public ObservableCollection<List<string>> TableCellData { get; set; } = new();
    public int TableHeaderRows { get; set; }
    public ObservableCollection<string> TableColumnHeaders { get; set; } = new();
}
