using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Models;
using ReportDataMaker.Services;
using ReportDataMaker.Services.ContextAdapter;
using ReportDataMaker.Services.DatabaseAdapter;
using ReportDataMaker.Services.ExcelAdapter;
using ReportDataMaker.Services.PdfExport;
using ReportDataMaker.ViewModels.Tabs;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.ViewModels;

/// <summary>主视图模型，管理模板加载、适配器添加和标签页切换</summary>
public class MainViewModel : ViewModelBase
{
    private readonly ITemplateLoaderService _templateLoader;
    private readonly IDataBindingService _dataBindingService;
    private readonly ITemplatePreviewService _previewService;
    private readonly IDialogService _dialogService;
    private readonly AdapterConfigStore _configStore;
    private readonly ExcelAdapterFactory _excelFactory;
    private readonly DatabaseAdapterFactory _dbFactory;
    private readonly ContextAdapterFactory _contextFactory;
    private readonly IPdfExportService _pdfExportService;
    private readonly BatchExportService _batchExportService;
    private readonly ExportHistoryStore _exportHistoryStore;
    private readonly DataBindingService _concreteDataBindingService;

    /// <summary>初始化主视图模型</summary>
    /// <param name="templateLoader">模板加载服务</param>
    /// <param name="dataBindingService">数据绑定服务</param>
    /// <param name="previewService">模板预览服务</param>
    /// <param name="dialogService">对话框服务</param>
    /// <param name="configStore">适配器配置存储</param>
    /// <param name="excelFactory">Excel适配器工厂</param>
    /// <param name="dbFactory">数据库适配器工厂</param>
    public MainViewModel(
        ITemplateLoaderService templateLoader,
        IDataBindingService dataBindingService,
        ITemplatePreviewService previewService,
        IDialogService dialogService,
        AdapterConfigStore configStore,
        ExcelAdapterFactory excelFactory,
        DatabaseAdapterFactory dbFactory,
        ContextAdapterFactory contextFactory,
        IPdfExportService pdfExportService,
        BatchExportService batchExportService,
        ExportHistoryStore exportHistoryStore,
        DataBindingService concreteDataBindingService)
    {
        _templateLoader = templateLoader;
        _dataBindingService = dataBindingService;
        _previewService = previewService;
        _dialogService = dialogService;
        _configStore = configStore;
        _excelFactory = excelFactory;
        _dbFactory = dbFactory;
        _contextFactory = contextFactory;
        _pdfExportService = pdfExportService;
        _batchExportService = batchExportService;
        _exportHistoryStore = exportHistoryStore;
        _concreteDataBindingService = concreteDataBindingService;

        Tabs = new ObservableCollection<TabViewModelBase>();
        Adapters = new ObservableCollection<AdapterItemViewModel>();

        LoadTemplateCommand = new RelayCommand(_ => ExecuteLoadTemplate());
        ToggleSidePanelCommand = new RelayCommand(_ => IsSidePanelExpanded = !IsSidePanelExpanded);
        AddExcelAdapterCommand = new RelayCommand(_ => ExecuteAddExcelAdapter(), _ => IsTemplateLoaded);
        AddDbAdapterCommand = new RelayCommand(_ => ExecuteAddDbAdapter(), _ => IsTemplateLoaded);
        EditContextCommand = new RelayCommand(_ => ExecuteEditContext(), _ => IsTemplateLoaded);
        ExportPdfCommand = new RelayCommand(_ => ExecuteExportPdf(), _ => IsTemplateLoaded);
        BatchExportCommand = new RelayCommand(_ => ExecuteBatchExport(), _ => IsTemplateLoaded);
        PrintPreviewCommand = new RelayCommand(_ => ExecutePrintPreview(), _ => IsTemplateLoaded);
        SaveCommand = new AsyncRelayCommand(ExecuteSaveAsync, _ => IsTemplateLoaded);
        ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());
    }

    private ExternalTemplateDefinition? _currentTemplate;
    /// <summary>当前加载的模板定义</summary>
    public ExternalTemplateDefinition? CurrentTemplate
    {
        get => _currentTemplate;
        private set { SetProperty(ref _currentTemplate, value); IsTemplateLoaded = value != null; }
    }

    private bool _isTemplateLoaded;
    /// <summary>是否已加载模板</summary>
    public bool IsTemplateLoaded { get => _isTemplateLoaded; private set => SetProperty(ref _isTemplateLoaded, value); }

    /// <summary>模板名称</summary>
    public string TemplateName => CurrentTemplate?.Name ?? "未加载模板";
    /// <summary>模板版本</summary>
    public string TemplateVersion => CurrentTemplate != null ? $"v{CurrentTemplate.Version}" : "";

    /// <summary>字段摘要信息</summary>
    public string FieldSummary
    {
        get
        {
            if (CurrentTemplate?.Elements == null) return "";
            var groups = CurrentTemplate.Elements.GroupBy(e => e.Group).ToDictionary(g => g.Key, g => g.Count());
            return string.Join(" | ", groups.Select(kv => $"{kv.Key}:{kv.Value}"));
        }
    }

    private bool _isSidePanelExpanded = true;
    /// <summary>侧边面板是否展开</summary>
    public bool IsSidePanelExpanded { get => _isSidePanelExpanded; set => SetProperty(ref _isSidePanelExpanded, value); }

    /// <summary>适配器项集合</summary>
    public ObservableCollection<AdapterItemViewModel> Adapters { get; }
    /// <summary>标签页集合</summary>
    public ObservableCollection<TabViewModelBase> Tabs { get; }

    private TabViewModelBase? _activeTab;
    /// <summary>当前活动的标签页</summary>
    public TabViewModelBase? ActiveTab { get => _activeTab; set => SetProperty(ref _activeTab, value); }

    private string _statusText = "就绪";
    /// <summary>状态栏文本</summary>
    public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

    private string _statusInfo = "";
    /// <summary>状态栏附加信息</summary>
    public string StatusInfo { get => _statusInfo; set => SetProperty(ref _statusInfo, value); }

    /// <summary>状态栏颜色</summary>
    public System.Windows.Media.Brush StatusColor => IsTemplateLoaded
        ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Gray;

    /// <summary>加载模板命令</summary>
    public RelayCommand LoadTemplateCommand { get; }
    /// <summary>切换侧边面板命令</summary>
    public RelayCommand ToggleSidePanelCommand { get; }
    /// <summary>添加Excel适配器命令</summary>
    public RelayCommand AddExcelAdapterCommand { get; }
    /// <summary>添加数据库适配器命令</summary>
    public RelayCommand AddDbAdapterCommand { get; }
    /// <summary>编辑上下文命令</summary>
    public RelayCommand EditContextCommand { get; }
    public RelayCommand ExportPdfCommand { get; }
    public RelayCommand BatchExportCommand { get; }
    public RelayCommand PrintPreviewCommand { get; }
    /// <summary>保存命令</summary>
    public AsyncRelayCommand SaveCommand { get; }
    /// <summary>退出命令</summary>
    public RelayCommand ExitCommand { get; }

    private void ExecuteLoadTemplate()
    {
        var filePath = _dialogService.OpenFile("JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*", "选择模板文件");
        if (string.IsNullOrEmpty(filePath)) return;
        try
        {
            var template = _templateLoader.LoadFromFile(filePath);
            LoadTemplate(template);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"加载模板失败: {ex.Message}", "错误");
        }
    }

    private void LoadTemplate(ExternalTemplateDefinition template)
    {
        CurrentTemplate = template;

        var contextConfig = _contextFactory.LoadProfile("default");
        var contextResult = _contextFactory.Fill(template, contextConfig);
        if (contextResult.Success && contextResult.Data.Count > 0)
        {
            foreach (var element in template.Elements)
            {
                if (element.Group == ElementGroup.Context && !string.IsNullOrEmpty(element.DataPath)
                    && contextResult.Data.TryGetValue(element.DataPath, out var value))
                {
                    element.DefaultValue = value?.ToString() ?? string.Empty;
                }
            }
        }

        StatusInfo = $"模板: {template.Name}";
        Tabs.Clear();
        Adapters.Clear();

        var dataEntryTab = new DataEntryTabViewModel(template, _dataBindingService);
        dataEntryTab.CloseRequested += OnTabCloseRequested;
        Tabs.Add(dataEntryTab);

        var previewTab = new PreviewTabViewModel(template, _previewService);
        previewTab.CloseRequested += OnTabCloseRequested;
        Tabs.Add(previewTab);

        var exportTab = new ExportTabViewModel(template, _pdfExportService, _batchExportService, _exportHistoryStore, _dialogService, _concreteDataBindingService);
        exportTab.CloseRequested += OnTabCloseRequested;
        Tabs.Add(exportTab);

        var savedConfigs = _configStore.Load(template.Name);
        foreach (var config in savedConfigs) AddAdapterTab(config);

        ActiveTab = Tabs[0];
        OnPropertyChanged(nameof(TemplateName));
        OnPropertyChanged(nameof(TemplateVersion));
        OnPropertyChanged(nameof(FieldSummary));
        OnPropertyChanged(nameof(StatusColor));
    }

    private void ExecuteAddExcelAdapter()
    {
        if (CurrentTemplate == null) return;

        var displayName = $"Excel适配器{Adapters.Count(a => a.Type == AdapterType.Excel) + 1}";
        var tab = new ExcelAdapterTabViewModel(CurrentTemplate, _excelFactory, _dialogService, displayName);
        tab.CloseRequested += OnTabCloseRequested;
        Tabs.Insert(Tabs.Count - 1, tab);

        var config = new ExcelAdapterConfig
        {
            Type = AdapterType.Excel,
            DisplayName = displayName,
            Mode = ImportMode.Single
        };
        Adapters.Add(new AdapterItemViewModel(config));
        ActiveTab = tab;
    }

    private void ExecuteAddDbAdapter()
    {
        if (CurrentTemplate == null) return;

        var displayName = $"数据库适配器{Adapters.Count(a => a.Type == AdapterType.Database) + 1}";
        var tab = new DatabaseAdapterTabViewModel(CurrentTemplate, _dbFactory, _dialogService, _configStore, displayName);
        tab.CloseRequested += OnTabCloseRequested;
        Tabs.Insert(Tabs.Count - 1, tab);

        var config = new DatabaseAdapterConfig
        {
            Type = AdapterType.Database,
            DisplayName = displayName
        };
        Adapters.Add(new AdapterItemViewModel(config));
        ActiveTab = tab;
    }

    private void ExecuteEditContext()
    {
        if (CurrentTemplate == null) return;
        var existingTab = Tabs.OfType<ContextAdapterTabViewModel>().FirstOrDefault();
        if (existingTab != null) { ActiveTab = existingTab; return; }
        var tab = new ContextAdapterTabViewModel(CurrentTemplate, _contextFactory, _dialogService);
        tab.CloseRequested += OnTabCloseRequested;
        Tabs.Insert(Tabs.Count - 1, tab);
        ActiveTab = tab;
    }

    private void ExecuteExportPdf()
    {
        if (CurrentTemplate == null) return;
        var existingTab = Tabs.OfType<ExportTabViewModel>().FirstOrDefault();
        if (existingTab != null) { ActiveTab = existingTab; return; }
        var tab = new ExportTabViewModel(CurrentTemplate, _pdfExportService, _batchExportService, _exportHistoryStore, _dialogService, _concreteDataBindingService);
        tab.CloseRequested += OnTabCloseRequested;
        Tabs.Add(tab);
        ActiveTab = tab;
    }

    private void ExecuteBatchExport()
    {
        ExecuteExportPdf();
    }

    private void ExecutePrintPreview()
    {
        ExecuteExportPdf();
    }

    private void AddAdapterTab(AdapterConfigBase config)
    {
        var tab = new DataEntryTabViewModel(CurrentTemplate!, _dataBindingService)
        {
            Title = $"适配器: {config.DisplayName}",
            IsClosable = true
        };
        tab.CloseRequested += OnTabCloseRequested;
        Tabs.Insert(Tabs.Count - 1, tab);
        Adapters.Add(new AdapterItemViewModel(config));
        ActiveTab = tab;
    }

    private void OnTabCloseRequested(TabViewModelBase tab)
    {
        if (tab.IsClosable)
        {
            Tabs.Remove(tab);
            var adapter = Adapters.FirstOrDefault(a => tab.Title.Contains(a.DisplayName));
            if (adapter != null) Adapters.Remove(adapter);
        }
    }

    private async Task ExecuteSaveAsync(object? parameter)
    {
        try
        {
            StatusText = "保存中...";
            await Task.Delay(100);
            _dialogService.ShowSuccess("配置已保存");
            StatusText = "已保存";
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"保存失败: {ex.Message}", "错误");
            StatusText = "保存失败";
        }
    }
}

/// <summary>适配器项视图模型，表示侧边栏中的适配器条目</summary>
public class AdapterItemViewModel : ViewModelBase
{
    /// <summary>适配器标识</summary>
    public string AdapterId { get; }
    /// <summary>显示名称</summary>
    public string DisplayName { get; }
    /// <summary>适配器类型</summary>
    public AdapterType Type { get; }

    /// <summary>初始化适配器项视图模型</summary>
    /// <param name="config">适配器配置基类</param>
    public AdapterItemViewModel(AdapterConfigBase config)
    {
        AdapterId = config.AdapterId;
        DisplayName = config.DisplayName;
        Type = config.Type;
    }
}
