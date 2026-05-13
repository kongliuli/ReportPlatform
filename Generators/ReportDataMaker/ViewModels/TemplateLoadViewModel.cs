using System.Collections.ObjectModel;
using System.Windows.Input;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Models;
using ReportDataMaker.Services;

namespace ReportDataMaker.ViewModels;

/// <summary>模板加载视图模型，管理模板文件浏览和最近模板列表</summary>
public class TemplateLoadViewModel : ViewModelBase
{
    private readonly ITemplateLoaderService _templateLoader;
    private readonly IDialogService _dialogService;

    /// <summary>初始化模板加载视图模型</summary>
    /// <param name="templateLoader">模板加载服务</param>
    /// <param name="dialogService">对话框服务</param>
    public TemplateLoadViewModel(ITemplateLoaderService templateLoader, IDialogService dialogService)
    {
        _templateLoader = templateLoader;
        _dialogService = dialogService;
        RecentTemplates = new ObservableCollection<RecentTemplate>();
        BrowseFileCommand = new RelayCommand(_ => ExecuteBrowseFile());
        CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(false));
    }

    /// <summary>最近使用模板列表</summary>
    public ObservableCollection<RecentTemplate> RecentTemplates { get; }
    /// <summary>浏览文件命令</summary>
    public ICommand BrowseFileCommand { get; }
    /// <summary>取消命令</summary>
    public ICommand CancelCommand { get; }

    /// <summary>模板选中事件</summary>
    public event Action<ExternalTemplateDefinition>? TemplateSelected;
    /// <summary>请求关闭事件</summary>
    public event Action<bool>? RequestClose;

    private void ExecuteBrowseFile()
    {
        FileLogger.Instance.WriteLine($"[TemplateLoadVM] 用户点击浏览文件");
        var filePath = _dialogService.OpenFile("JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*", "选择模板文件");
        
        FileLogger.Instance.WriteLine($"[TemplateLoadVM] 用户选择文件: {filePath}");
        
        if (!string.IsNullOrEmpty(filePath))
        {
            try
            {
                FileLogger.Instance.WriteLine($"[TemplateLoadVM] 开始加载模板...");
                var template = _templateLoader.LoadFromFile(filePath);
                FileLogger.Instance.WriteLine($"[TemplateLoadVM] 模板加载成功，准备触发事件");
                TemplateSelected?.Invoke(template);
                RequestClose?.Invoke(true);
                FileLogger.Instance.WriteLine($"[TemplateLoadVM] 事件触发完成");
            }
            catch (Exception ex)
            {
                FileLogger.Instance.WriteLine($"[TemplateLoadVM] 加载失败: {ex.GetType().Name} - {ex.Message}");
                _dialogService.ShowError($"加载模板失败: {ex.Message}", "错误");
            }
        }
        else
        {
            FileLogger.Instance.WriteLine($"[TemplateLoadVM] 用户取消选择文件");
        }
    }
}

/// <summary>最近使用模板，记录最近打开的模板信息</summary>
public class RecentTemplate
{
    /// <summary>模板名称</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>文件路径</summary>
    public string FilePath { get; set; } = string.Empty;
    /// <summary>最后使用时间</summary>
    public DateTime LastUsed { get; set; }
}
