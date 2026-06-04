using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReportDataMaker.Services;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.ViewModels.Tabs;

public partial class DataEntryTabViewModel : MainTabViewModel
{
    [ObservableProperty] private ObservableCollection<EditableFieldItem> _fields = new();
    [ObservableProperty] private EditableFieldItem? _selectedField;

    public DataEntryTabViewModel(MainViewModel mainViewModel) : base(mainViewModel) { }

    public override void OnTemplateChanged()
    {
        base.OnTemplateChanged();
        if (CurrentTemplate == null) return;

        Fields.Clear();
        foreach (var element in CurrentTemplate.Elements)
        {
            if (element.Group != ElementGroup.Editable) continue;

            var field = new EditableFieldItem
            {
                ElementId = element.Id,
                DataPath = element.DataPath ?? element.Id,
                Label = element.Label ?? element.Id,
                FieldType = element switch
                {
                    TextElement => "文本",
                    NumberElement => "数字",
                    DateElement => "日期",
                    DropdownElement => "下拉",
                    CheckboxElement => "复选",
                    RadioElement => "单选",
                    TableElement => "表格",
                    SignatureElement => "签名",
                    _ => "其他"
                },
                Value = element.DefaultValue ?? string.Empty,
                IsRequired = element.IsRequired
            };
            Fields.Add(field);
        }
    }

    [RelayCommand]
    private void ApplyData()
    {
        if (CurrentTemplate == null) return;
        var data = Fields.Where(f => !string.IsNullOrEmpty(f.DataPath))
            .ToDictionary(f => f.DataPath, f => (object)f.Value);
        MainViewModel._dataBindingService.ApplyData(CurrentTemplate, data);
        StatusText = $"已应用 {data.Count} 个字段的数据";
    }
}

public class EditableFieldItem
{
    public string ElementId { get; set; } = string.Empty;
    public string DataPath { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
}
