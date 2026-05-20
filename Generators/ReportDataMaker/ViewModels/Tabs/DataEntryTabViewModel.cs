using System.Collections.ObjectModel;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Models;
using ReportDataMaker.Services;

namespace ReportDataMaker.ViewModels.Tabs;

/// <summary>数据录入标签页视图模型，管理模板字段的数据录入</summary>
public class DataEntryTabViewModel : TabViewModelBase
{
    private readonly IDataBindingService _dataBindingService;

    /// <summary>关联的模板定义</summary>
    public ExternalTemplateDefinition? Template { get; }
    /// <summary>字段视图模型集合</summary>
    public ObservableCollection<FieldViewModel> Fields { get; } = new();

    /// <summary>初始化数据录入标签页视图模型</summary>
    /// <param name="template">外部模板定义</param>
    /// <param name="dataBindingService">数据绑定服务</param>
    public DataEntryTabViewModel(ExternalTemplateDefinition template, IDataBindingService dataBindingService)
    {
        _dataBindingService = dataBindingService;
        Template = template;
        Title = "数据录入";
        IsClosable = false;
        LoadFields();
    }

    private void LoadFields()
    {
        if (Template?.Elements == null) return;
        Fields.Clear();
        foreach (var element in Template.Elements)
        {
            // 表格元素：即使 DataPath 为空也展示（自动使用 Id 作为 DataPath）
            if (element is ExternalTableElement table)
            {
                var tableField = new FieldViewModel
                {
                    ElementId = element.Id,
                    Label = element.Label ?? element.Id ?? string.Empty,
                    DataPath = element.DataPath ?? element.Id ?? string.Empty,
                    FieldType = FieldDataType.Table,
                    TableRows = table.Rows,
                    TableColumns = table.Columns,
                    TableHeaderRows = table.HeaderRows,
                    TableCellData = table.CellData ?? new List<List<string>>()
                };
                Fields.Add(tableField);
                continue;
            }

            // 非表格元素：必须有 DataPath 才展示
            if (string.IsNullOrEmpty(element.DataPath))
                continue;

            var field = new FieldViewModel
            {
                ElementId = element.Id,
                Label = element.Label ?? string.Empty,
                DataPath = element.DataPath ?? string.Empty,
                Value = element.DefaultValue
            };

            Fields.Add(field);
        }
    }

    public void ApplyDataFromAdapter(string dataPath, object value)
    {
        var field = Fields.FirstOrDefault(f => f.DataPath == dataPath);
        if (field == null) return;

        if (field.FieldType == FieldDataType.Table && value is List<List<string>> cellData)
        {
            field.TableCellData = cellData;
            return;
        }

        field.Value = value?.ToString() ?? string.Empty;
    }
}

public enum FieldDataType { Text, Dropdown, Number, Date, Boolean, ReadOnly, Table }

/// <summary>字段视图模型，表示单个可编辑字段</summary>
public class FieldViewModel : ViewModelBase
{
    public string ElementId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string DataPath { get; set; } = string.Empty;

    private string _value = string.Empty;
    public string Value { get => _value; set => SetProperty(ref _value, value); }

    public FieldDataType FieldType { get; set; } = FieldDataType.Text;
    public bool IsMultiLine { get; set; }

    public List<string> Options { get; set; } = new();
    public string GroupName { get; set; } = string.Empty;

    private string _selectedOption = string.Empty;
    public string SelectedOption
    {
        get => _selectedOption;
        set { if (SetProperty(ref _selectedOption, value)) Value = value ?? string.Empty; }
    }

    public string Unit { get; set; } = string.Empty;
    public int DecimalPlaces { get; set; } = 2;

    public string DateFormat { get; set; } = "yyyy-MM-dd";
    private DateTime? _dateValue;
    public DateTime? DateValue
    {
        get => _dateValue;
        set { if (SetProperty(ref _dateValue, value)) Value = value?.ToString(DateFormat) ?? string.Empty; }
    }

    private bool _isChecked;
    public bool IsChecked
    {
        get => _isChecked;
        set { if (SetProperty(ref _isChecked, value)) Value = value ? "true" : "false"; }
    }

    public int TableRows { get; set; }
    public int TableColumns { get; set; }
    public int TableHeaderRows { get; set; } = 1;
    public List<List<string>> TableCellData { get; set; } = new();
}

/// <summary>表单分组视图模型</summary>
public class SectionViewModel : ViewModelBase
{
    public string Title { get; set; } = string.Empty;
    public ObservableCollection<FieldViewModel> Fields { get; } = new();
    private bool _isExpanded = true;
    public bool IsExpanded { get => _isExpanded; set => SetProperty(ref _isExpanded, value); }
}
