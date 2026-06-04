using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReportDataMaker.Services;
using ReportDataMaker.Services.DatabaseAdapter;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.ViewModels.Tabs;

public partial class DatabaseAdapterTabViewModel : MainTabViewModel
{
    [ObservableProperty] private string _connectionString = string.Empty;
    [ObservableProperty] private string _query = string.Empty;
    [ObservableProperty] private ObservableCollection<DatabaseFieldMapping> _fieldMappings = new();
    [ObservableProperty] private ObservableCollection<string> _selectedColumnNames = new();

    private readonly AdapterRegistry _registry;

    public DatabaseAdapterTabViewModel(MainViewModel mainViewModel, AdapterRegistry registry) : base(mainViewModel) { _registry = registry; }

    public override void OnTemplateChanged()
    {
        base.OnTemplateChanged();
        if (CurrentTemplate == null) return;

        FieldMappings.Clear();
        foreach (var element in CurrentTemplate.Elements)
        {
            if (string.IsNullOrEmpty(element.DataPath)) continue;
            FieldMappings.Add(new DatabaseFieldMapping
            {
                DataPath = element.DataPath,
                Label = element.Label ?? element.Id,
                DbColumn = string.Empty
            });
        }
    }

    [RelayCommand]
    private void TestConnection()
    {
        StatusText = "数据库连接测试功能待实现";
    }

    [RelayCommand]
    private void ImportFromDatabase()
    {
        if (CurrentTemplate == null) return;
        StatusText = "数据库导入功能待实现";
    }
}

public class DatabaseFieldMapping
{
    public string DataPath { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string DbColumn { get; set; } = string.Empty;
}
