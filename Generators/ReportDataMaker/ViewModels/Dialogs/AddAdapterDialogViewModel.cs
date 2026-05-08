using ReportDataMaker.Infrastructure;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.ViewModels.Dialogs;

public class AddAdapterDialogViewModel : ViewModelBase
{
    private string _adapterName = "新适配器";
    public string AdapterName { get => _adapterName; set => SetProperty(ref _adapterName, value); }

    private AdapterType? _selectedAdapterType;
    public AdapterType? SelectedAdapterType { get => _selectedAdapterType; set => SetProperty(ref _selectedAdapterType, value); }

    public IEnumerable<AdapterTypeItem> AvailableAdapterTypes { get; } = new[]
    {
        new AdapterTypeItem { Type = AdapterType.Excel, Name = "Excel 导入", Description = "从 Excel 文件导入数据" },
        new AdapterTypeItem { Type = AdapterType.Database, Name = "数据库查询", Description = "从数据库查询数据" },
        new AdapterTypeItem { Type = AdapterType.Api, Name = "API 调用", Description = "从 HTTP API 获取数据", IsEnabled = false }
    };

    public RelayCommand ConfirmCommand { get; }
    public RelayCommand CancelCommand { get; }
    public event Action<bool>? RequestClose;

    public AddAdapterDialogViewModel()
    {
        ConfirmCommand = new RelayCommand(_ =>
        {
            if (SelectedAdapterType != null && !string.IsNullOrWhiteSpace(AdapterName))
                RequestClose?.Invoke(true);
        });
        CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(false));
    }
}

public class AdapterTypeItem
{
    public AdapterType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
}
