using ReportDataMaker.Infrastructure;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.ViewModels.Dialogs;

/// <summary>添加适配器对话框视图模型，管理适配器类型选择和命名</summary>
public class AddAdapterDialogViewModel : ViewModelBase
{
    private string _adapterName = "新适配器";
    /// <summary>适配器名称</summary>
    public string AdapterName { get => _adapterName; set => SetProperty(ref _adapterName, value); }

    private AdapterType? _selectedAdapterType;
    /// <summary>选中的适配器类型</summary>
    public AdapterType? SelectedAdapterType { get => _selectedAdapterType; set => SetProperty(ref _selectedAdapterType, value); }

    /// <summary>可用的适配器类型列表</summary>
    public IEnumerable<AdapterTypeItem> AvailableAdapterTypes { get; } = new[]
    {
        new AdapterTypeItem { Type = AdapterType.Excel, Name = "Excel 导入", Description = "从 Excel 文件导入数据" },
        new AdapterTypeItem { Type = AdapterType.Database, Name = "数据库查询", Description = "从数据库查询数据" },
        new AdapterTypeItem { Type = AdapterType.Api, Name = "API 调用", Description = "从 HTTP API 获取数据", IsEnabled = false }
    };

    /// <summary>确认命令</summary>
    public RelayCommand ConfirmCommand { get; }
    /// <summary>取消命令</summary>
    public RelayCommand CancelCommand { get; }
    /// <summary>请求关闭事件</summary>
    public event Action<bool>? RequestClose;

    /// <summary>初始化添加适配器对话框视图模型</summary>
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

/// <summary>适配器类型项，表示对话框中可选的适配器类型</summary>
public class AdapterTypeItem
{
    /// <summary>适配器类型</summary>
    public AdapterType Type { get; set; }
    /// <summary>类型名称</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>类型描述</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>是否可用</summary>
    public bool IsEnabled { get; set; } = true;
}
