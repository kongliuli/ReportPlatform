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
                    Label = element.Label,
                    DataPath = element.DataPath,
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

/// <summary>字段视图模型，表示单个可编辑字段</summary>
public class FieldViewModel : ViewModelBase
{
    /// <summary>元素标识</summary>
    public string ElementId { get; set; } = string.Empty;
    /// <summary>字段标签</summary>
    public string Label { get; set; } = string.Empty;
    /// <summary>数据路径</summary>
    public string DataPath { get; set; } = string.Empty;

    private string _value = string.Empty;
    /// <summary>字段值</summary>
    public string Value { get => _value; set => SetProperty(ref _value, value); }
}
