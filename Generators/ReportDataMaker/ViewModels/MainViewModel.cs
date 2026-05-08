using System.Collections.ObjectModel;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Models;
using ReportDataMaker.Services;
using ReportDataMaker.Services.DatabaseAdapter;
using ReportDataMaker.Services.ExcelAdapter;
using ReportDataMaker.ViewModels.Tabs;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ITemplateLoaderService _templateLoader;
    private readonly IDataBindingService _dataBindingService;
    private readonly ITemplatePreviewService _previewService;
    private readonly IDialogService _dialogService;
    private readonly AdapterConfigStore _configStore;
    private readonly ExcelAdapterFactory _excelFactory;
    private readonly DatabaseAdapterFactory _dbFactory;

    public MainViewModel(
        ITemplateLoaderService templateLoader,
        IDataBindingService dataBindingService,
        ITemplatePreviewService previewService,
        IDialogService dialogService,
        AdapterConfigStore configStore,
        ExcelAdapterFactory excelFactory,
        DatabaseAdapterFactory dbFactory)
    {
        _templateLoader = templateLoader;
        _dataBindingService = dataBindingService;
        _previewService = previewService;
        _dialogService = dialogService;
        _configStore = configStore;
        _excelFactory = excelFactory;
        _dbFactory = dbFactory;

        Tabs = new ObservableCollection<TabViewModelBase>();
        Adapters = new ObservableCollection<AdapterItemViewModel>();

        LoadTemplateCommand = new RelayCommand(_ => ExecuteLoadTemplate());
        ToggleSidePanelCommand = new RelayCommand(_ => IsSidePanelExpanded = !IsSidePanelExpanded);
        AddExcelAdapterCommand = new RelayCommand(_ => ExecuteAddExcelAdapter(), _ => IsTemplateLoaded);
        AddDbAdapterCommand = new RelayCommand(_ => ExecuteAddDbAdapter(), _ => IsTemplateLoaded);
        SaveCommand = new AsyncRelayCommand(ExecuteSaveAsync, _ => IsTemplateLoaded);
        ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());
    }

    private ExternalTemplateDefinition? _currentTemplate;
    public ExternalTemplateDefinition? CurrentTemplate
    {
        get => _currentTemplate;
        private set { SetProperty(ref _currentTemplate, value); IsTemplateLoaded = value != null; }
    }

    private bool _isTemplateLoaded;
    public bool IsTemplateLoaded { get => _isTemplateLoaded; private set => SetProperty(ref _isTemplateLoaded, value); }

    public string TemplateName => CurrentTemplate?.Name ?? "未加载模板";
    public string TemplateVersion => CurrentTemplate != null ? $"v{CurrentTemplate.Version}" : "";

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
    public bool IsSidePanelExpanded { get => _isSidePanelExpanded; set => SetProperty(ref _isSidePanelExpanded, value); }

    public ObservableCollection<AdapterItemViewModel> Adapters { get; }
    public ObservableCollection<TabViewModelBase> Tabs { get; }

    private TabViewModelBase? _activeTab;
    public TabViewModelBase? ActiveTab { get => _activeTab; set => SetProperty(ref _activeTab, value); }

    private string _statusText = "就绪";
    public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

    private string _statusInfo = "";
    public string StatusInfo { get => _statusInfo; set => SetProperty(ref _statusInfo, value); }

    public System.Windows.Media.Brush StatusColor => IsTemplateLoaded
        ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Gray;

    public RelayCommand LoadTemplateCommand { get; }
    public RelayCommand ToggleSidePanelCommand { get; }
    public RelayCommand AddExcelAdapterCommand { get; }
    public RelayCommand AddDbAdapterCommand { get; }
    public AsyncRelayCommand SaveCommand { get; }
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
        StatusInfo = $"模板: {template.Name}";
        Tabs.Clear();
        Adapters.Clear();

        var dataEntryTab = new DataEntryTabViewModel(template, _dataBindingService);
        dataEntryTab.CloseRequested += OnTabCloseRequested;
        Tabs.Add(dataEntryTab);

        var previewTab = new PreviewTabViewModel(template, _previewService);
        previewTab.CloseRequested += OnTabCloseRequested;
        Tabs.Add(previewTab);

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

public class AdapterItemViewModel : ViewModelBase
{
    public string AdapterId { get; }
    public string DisplayName { get; }
    public AdapterType Type { get; }

    public AdapterItemViewModel(AdapterConfigBase config)
    {
        AdapterId = config.AdapterId;
        DisplayName = config.DisplayName;
        Type = config.Type;
    }
}
