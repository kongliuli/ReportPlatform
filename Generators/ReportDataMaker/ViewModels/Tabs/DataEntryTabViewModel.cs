using System.Collections.ObjectModel;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Models;
using ReportDataMaker.Services;

namespace ReportDataMaker.ViewModels.Tabs;

public class DataEntryTabViewModel : TabViewModelBase
{
    private readonly IDataBindingService _dataBindingService;

    public ExternalTemplateDefinition? Template { get; }
    public ObservableCollection<FieldViewModel> Fields { get; } = new();

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

    public void ApplyDataFromAdapter(string dataPath, object value)
    {
        var field = Fields.FirstOrDefault(f => f.DataPath == dataPath);
        if (field != null) field.Value = value?.ToString() ?? string.Empty;
    }
}

public class FieldViewModel : ViewModelBase
{
    public string ElementId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string DataPath { get; set; } = string.Empty;

    private string _value = string.Empty;
    public string Value { get => _value; set => SetProperty(ref _value, value); }
}
