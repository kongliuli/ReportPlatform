using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.ViewModels.Dialogs;

public partial class AddAdapterDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _adapterName = "新适配器";

    [ObservableProperty]
    private AdapterType? _selectedAdapterType;

    public IEnumerable<AdapterTypeItem> AvailableAdapterTypes { get; } = new[]
    {
        new AdapterTypeItem { Type = AdapterType.Excel, Name = "Excel 导入", Description = "从 Excel 文件导入数据" },
        new AdapterTypeItem { Type = AdapterType.Database, Name = "数据库查询", Description = "从数据库查询数据" },
        new AdapterTypeItem { Type = AdapterType.Api, Name = "API 调用", Description = "从 HTTP API 获取数据", IsEnabled = false }
    };

    [RelayCommand]
    private void Confirm()
    {
        if (SelectedAdapterType != null && !string.IsNullOrWhiteSpace(AdapterName))
            RequestClose?.Invoke(true);
    }

    [RelayCommand]
    private void Cancel() => RequestClose?.Invoke(false);

    public event Action<bool>? RequestClose;
}

public class AdapterTypeItem
{
    public AdapterType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
}
