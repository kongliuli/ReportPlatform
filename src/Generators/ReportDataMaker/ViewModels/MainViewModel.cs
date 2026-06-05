using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Services;
using Xinglin.ReportEditor.Contracts.Models.Adapters;
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
    [ObservableProperty] private string _statusColor = "#4CAF50";
    [ObservableProperty] private string _statusInfo = string.Empty;

    private readonly ITemplateLoaderService _templateLoaderService;
    private readonly IDataBindingService _dataBindingService;
    private readonly ITemplatePreviewService _previewService;
    private readonly IPdfExportService _pdfExportService;
    private readonly AdapterRegistry _adapterRegistry;
    private readonly IDialogService _dialogService;

    public MainViewModel(
        ITemplateLoaderService templateLoaderService,
        IDataBindingService dataBindingService,
        ITemplatePreviewService previewService,
        IPdfExportService pdfExportService,
        AdapterRegistry adapterRegistry,
        IDialogService dialogService,
        SidePanelViewModel sidePanel)
    {
        _templateLoaderService = templateLoaderService;
        _dataBindingService = dataBindingService;
        _previewService = previewService;
        _pdfExportService = pdfExportService;
        _adapterRegistry = adapterRegistry;
        _dialogService = dialogService;
        SidePanel = sidePanel;

        InitializeTabs();
        LoadAdapters();
    }

    private void LoadAdapters()
    {
        Adapters.Clear();
        foreach (var plugin in _adapterRegistry.GetAllPlugins())
            Adapters.Add(plugin);
    }

    private void InitializeTabs()
    {
        Tabs.Clear();
        Tabs.Add(new DataEntryTabViewModel(this, _dataBindingService) { Header = "数据录入" });
        Tabs.Add(new PreviewTabViewModel(this) { Header = "预览" });
        Tabs.Add(new ExportTabViewModel(this, _dataBindingService, _pdfExportService) { Header = "导出" });
        Tabs.Add(CreateClosableTab(new ContextAdapterTabViewModel(this, _adapterRegistry) { Header = "上下文适配" }));
        Tabs.Add(CreateClosableTab(new ExcelAdapterTabViewModel(this, _adapterRegistry) { Header = "Excel适配" }));
        Tabs.Add(CreateClosableTab(new DatabaseAdapterTabViewModel(this, _adapterRegistry) { Header = "数据库适配" }));
        SelectedTab = Tabs.FirstOrDefault();
    }

    private T CreateClosableTab<T>(T tab) where T : TabViewModelBase
    {
        tab.IsClosable = true;
        tab.CloseRequested += OnTabCloseRequested;
        return tab;
    }

    private void OnTabCloseRequested(TabViewModelBase tab)
    {
        if (tab.IsClosable)
        {
            Tabs.Remove(tab);
            if (SelectedTab == null && Tabs.Count > 0)
                SelectedTab = Tabs[0];
        }
    }

    [RelayCommand]
    private async Task LoadTemplate()
    {
        var filePath = _dialogService.OpenFile("模板文件 (*.json)|*.json|所有文件 (*.*)|*.*", "选择模板文件");
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
    internal void RefreshPreview()
    {
        if (CurrentTemplate == null) return;
        foreach (var tab in Tabs.OfType<PreviewTabViewModel>())
            tab.RefreshPreview();
    }

    public string TemplateName => CurrentTemplate?.Name ?? "未加载模板";
    public string TemplateVersion => CurrentTemplate?.Version.ToString() ?? "";
    public string FieldSummary => CurrentTemplate != null
        ? $"{CurrentTemplate.Elements?.Count ?? 0} 个元素"
        : "";
    public ObservableCollection<IAdapterPlugin> Adapters { get; } = new();

    public TabViewModelBase? ActiveTab
    {
        get => SelectedTab;
        set => SelectedTab = value;
    }

    partial void OnCurrentTemplateChanged(TemplateDefinition? value)
    {
        OnPropertyChanged(nameof(TemplateName));
        OnPropertyChanged(nameof(TemplateVersion));
        OnPropertyChanged(nameof(FieldSummary));
    }

    partial void OnSelectedTabChanged(TabViewModelBase? value)
    {
        OnPropertyChanged(nameof(ActiveTab));
    }

    partial void OnStatusTextChanged(string value)
    {
        StatusColor = value.Contains("失败") || value.Contains("错误") ? "#F44336" : "#4CAF50";
    }

    public SidePanelViewModel SidePanel { get; }

    [RelayCommand]
    private void Save()
    {
        if (CurrentTemplate == null)
        {
            MessageBox.Show("请先加载模板", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        StatusText = "配置已保存";
        StatusInfo = $"模板: {CurrentTemplate.Name}";
    }

    [RelayCommand]
    private void Exit() => Application.Current.Shutdown();

    [RelayCommand]
    private void EditContext()
    {
        var contextTab = Tabs.OfType<ContextAdapterTabViewModel>().FirstOrDefault();
        if (contextTab != null)
            SelectedTab = contextTab;
    }

    [RelayCommand]
    private void BatchExport()
    {
        StatusText = "批量导出功能尚未实现";
    }
}
