using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Services;
using ReportDataMaker.Services.PdfExport;
using ReportDataMaker.ViewModels.Tabs;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private string _title = "报告数据制作工具";
    [ObservableProperty] private string _statusText = "就绪";
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private TemplateDefinition? _currentTemplate;
    [ObservableProperty] private string _templateFilePath = string.Empty;
    [ObservableProperty] private ObservableCollection<TabViewModelBase> _tabs = new();
    [ObservableProperty] private TabViewModelBase? _selectedTab;

    private readonly ITemplateLoaderService _templateLoaderService;
    private readonly IDataBindingService _dataBindingService;
    private readonly ITemplatePreviewService _previewService;
    private readonly IPdfExportService _pdfExportService;

    public MainViewModel(
        ITemplateLoaderService templateLoaderService,
        IDataBindingService dataBindingService,
        ITemplatePreviewService previewService,
        IPdfExportService pdfExportService)
    {
        _templateLoaderService = templateLoaderService;
        _dataBindingService = dataBindingService;
        _previewService = previewService;
        _pdfExportService = pdfExportService;

        InitializeTabs();
    }

    private void InitializeTabs()
    {
        Tabs.Clear();
        Tabs.Add(new DataEntryTabViewModel(this) { Header = "数据录入" });
        Tabs.Add(new PreviewTabViewModel(this) { Header = "预览" });
        Tabs.Add(new ExportTabViewModel(this) { Header = "导出" });
        Tabs.Add(new ContextAdapterTabViewModel(this) { Header = "上下文适配" });
        Tabs.Add(new ExcelAdapterTabViewModel(this) { Header = "Excel适配" });
        Tabs.Add(new DatabaseAdapterTabViewModel(this) { Header = "数据库适配" });
        SelectedTab = Tabs.FirstOrDefault();
    }

    [RelayCommand]
    private async Task LoadTemplate()
    {
        var filePath = _dialogService?.OpenFile("模板文件 (*.json)|*.json|所有文件 (*.*)|*.*", "选择模板文件");
        if (string.IsNullOrEmpty(filePath)) return;

        IsBusy = true;
        StatusText = "正在加载模板...";
        try
        {
            CurrentTemplate = await Task.Run(() => _templateLoaderService.LoadFromFile(filePath));
            TemplateFilePath = filePath;
            StatusText = $"已加载模板: {CurrentTemplate.Name} ({CurrentTemplate.Elements?.Count ?? 0} 个元素)";
            foreach (var tab in Tabs.OfType<MainTabViewModel>())
                tab.OnTemplateChanged();
        }
        catch (Exception ex)
        {
            StatusText = $"加载失败: {ex.Message}";
            MessageBox.Show($"加载模板失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void RefreshPreview()
    {
        if (CurrentTemplate == null) return;
        foreach (var tab in Tabs.OfType<PreviewTabViewModel>())
            tab.RefreshPreview();
    }

    [RelayCommand]
    private async Task ExportPdf()
    {
        if (CurrentTemplate == null) return;
        var data = _dataBindingService.ExtractData(CurrentTemplate);
        try
        {
            var pdfBytes = await Task.Run(() => _pdfExportService.RenderToPdf(CurrentTemplate, data));
            var filePath = _dialogService?.SaveFile("PDF 文件 (*.pdf)|*.pdf", "保存PDF", CurrentTemplate.Name);
            if (!string.IsNullOrEmpty(filePath))
            {
                await File.WriteAllBytesAsync(filePath, pdfBytes);
                StatusText = $"已导出: {filePath}";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"导出失败: {ex.Message}";
            MessageBox.Show($"导出PDF失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private IDialogService? _dialogService;
    public void SetDialogService(IDialogService dialogService) => _dialogService = dialogService;
}
