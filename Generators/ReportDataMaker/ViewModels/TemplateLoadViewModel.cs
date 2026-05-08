using System.Collections.ObjectModel;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Models;
using ReportDataMaker.Services;

namespace ReportDataMaker.ViewModels;

public class TemplateLoadViewModel : ViewModelBase
{
    private readonly ITemplateLoaderService _templateLoader;
    private readonly IDialogService _dialogService;

    public TemplateLoadViewModel(ITemplateLoaderService templateLoader, IDialogService dialogService)
    {
        _templateLoader = templateLoader;
        _dialogService = dialogService;
        RecentTemplates = new ObservableCollection<RecentTemplate>();
        BrowseFileCommand = new RelayCommand(_ => ExecuteBrowseFile());
        CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(false));
    }

    public ObservableCollection<RecentTemplate> RecentTemplates { get; }
    public ICommand BrowseFileCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action<ExternalTemplateDefinition>? TemplateSelected;
    public event Action<bool>? RequestClose;

    private void ExecuteBrowseFile()
    {
        var filePath = _dialogService.OpenFile("JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*", "选择模板文件");
        if (!string.IsNullOrEmpty(filePath))
        {
            try
            {
                var template = _templateLoader.LoadFromFile(filePath);
                TemplateSelected?.Invoke(template);
                RequestClose?.Invoke(true);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"加载模板失败: {ex.Message}", "错误");
            }
        }
    }
}

public class RecentTemplate
{
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime LastUsed { get; set; }
}
