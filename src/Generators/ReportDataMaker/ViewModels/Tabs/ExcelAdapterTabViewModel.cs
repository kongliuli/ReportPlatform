using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReportDataMaker.Services;
using ReportDataMaker.Adapter.Excel.Services;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.ViewModels.Tabs;

public partial class ExcelAdapterTabViewModel : MainTabViewModel
{
    [ObservableProperty] private string _excelFilePath = string.Empty;
    [ObservableProperty] private string _sheetName = string.Empty;
    [ObservableProperty] private bool _hasHeader = true;
    [ObservableProperty] private ObservableCollection<ExcelFieldMapping> _fieldMappings = new();

    private readonly AdapterRegistry _registry;

    public ExcelAdapterTabViewModel(MainViewModel mainViewModel, AdapterRegistry registry) : base(mainViewModel) { _registry = registry; }

    public override void OnTemplateChanged()
    {
        base.OnTemplateChanged();
        if (CurrentTemplate == null) return;

        var flattenService = (TemplateFlattenService?)_registry.GetByType("excel")?.CreateService(CurrentTemplate);
        if (flattenService == null)
        {
            StatusText = "Excel适配器未找到";
            return;
        }
        var fields = flattenService.FlattenTemplate(CurrentTemplate);

        FieldMappings.Clear();
        foreach (var field in fields)
        {
            FieldMappings.Add(new ExcelFieldMapping
            {
                DataPath = field.DataPath,
                Label = field.Label,
                FieldType = field.DataType.ToString(),
                ExcelColumn = string.Empty
            });
        }
    }

    [RelayCommand]
    private void ImportFromExcel()
    {
        if (CurrentTemplate == null) return;
        StatusText = "Excel导入功能待实现";
    }

    [RelayCommand]
    private void ExportToExcel()
    {
        if (CurrentTemplate == null) return;
        StatusText = "Excel导出功能待实现";
    }
}

public class ExcelFieldMapping
{
    public string DataPath { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
    public string ExcelColumn { get; set; } = string.Empty;
}
