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
            if (!string.IsNullOrEmpty(element.DataPath))
            {
                Fields.Add(new FieldViewModel
                {
                    ElementId = element.Id,
                    Label = element.Label ?? string.Empty,
                    DataPath = element.DataPath ?? string.Empty,
                    Value = element.DefaultValue
                });
            }
        }
    }

    /// <summary>从适配器应用数据到指定字段</summary>
    /// <param name="dataPath">数据路径</param>
    /// <param name="value">数据值</param>
    public void ApplyDataFromAdapter(string dataPath, object value)
    {
        var field = Fields.FirstOrDefault(f => f.DataPath == dataPath);
        if (field != null) field.Value = value?.ToString() ?? string.Empty;
    }
}

public enum FieldDataType { Text, Dropdown, Number, Date, Boolean, ReadOnly }

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
}

/// <summary>表单分组视图模型</summary>
public class SectionViewModel : ViewModelBase
{
    public string Title { get; set; } = string.Empty;
    public ObservableCollection<FieldViewModel> Fields { get; } = new();
    private bool _isExpanded = true;
    public bool IsExpanded { get => _isExpanded; set => SetProperty(ref _isExpanded, value); }
}
