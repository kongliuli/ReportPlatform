using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Models;
using ReportDataMaker.Services.ContextAdapter;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.ViewModels.Tabs;

public class ContextAdapterTabViewModel : TabViewModelBase
{
    private readonly ExternalTemplateDefinition _template;
    private readonly ContextAdapterFactory _factory;
    private readonly IDialogService _dialogService;

    public ContextAdapterTabViewModel(
        ExternalTemplateDefinition template,
        ContextAdapterFactory factory,
        IDialogService dialogService)
    {
        FileLogger.Instance.WriteLine("[LOG] ===== ContextAdapterTabViewModel 构造 =====");
        _template = template;
        _factory = factory;
        _dialogService = dialogService;
        Title = "上下文配置";
        IsClosable = true;

        SourceList = Enum.GetValues(typeof(ContextValueSource)).Cast<ContextValueSource>().ToList();
        FileLogger.Instance.WriteLine($"[LOG] ContextVM: SourceList 已加载, 共 {SourceList.Count} 个源类型");

        _selectedProfileName = "default";

        NewProfileCommand = new RelayCommand(_ => ExecuteNewProfile());
        DeleteProfileCommand = new RelayCommand(_ => ExecuteDeleteProfile(), _ => !string.IsNullOrEmpty(SelectedProfileName) && AvailableProfiles.Count > 0);
        AddStaticValueCommand = new RelayCommand(_ => ExecuteAddStaticValue());
        RemoveStaticValueCommand = new RelayCommand(p => { if (p is ContextStaticItem item) StaticValues.Remove(item); });
        AddDynamicRuleCommand = new RelayCommand(_ => ExecuteAddDynamicRule());
        RemoveDynamicRuleCommand = new RelayCommand(p => { if (p is DynamicContextRule rule) DynamicRules.Remove(rule); });
        DetectUnconfiguredCommand = new RelayCommand(_ => ExecuteDetectUnconfigured());
        PreviewCommand = new RelayCommand(_ => ExecutePreview());
        SaveCommand = new RelayCommand(_ => ExecuteSave());
        ApplyCommand = new RelayCommand(_ => ExecuteApply());

        FileLogger.Instance.WriteLine("[LOG] ContextVM: 调度 DeferredInit...");
        // Defer file I/O to avoid blocking UI thread in constructor
        System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
            System.Windows.Threading.DispatcherPriority.Loaded,
            new Action(DeferredInit));
        FileLogger.Instance.WriteLine("[LOG] ===== ContextAdapterTabViewModel 构造完成 =====");
    }

    private void DeferredInit()
    {
        FileLogger.Instance.WriteLine("[LOG] ===== ContextVM.DeferredInit 开始 =====");
        try
        {
            FileLogger.Instance.WriteLine("[LOG] DeferredInit: 调用 GetProfileNames...");
            var profiles = _factory.GetProfileNames();
            FileLogger.Instance.WriteLine($"[LOG] DeferredInit: GetProfileNames 返回 {profiles?.Count ?? 0} 个");
            AvailableProfiles = profiles ?? new List<string>();
            
            if (AvailableProfiles.Count > 0)
            {
                _selectedProfileName = AvailableProfiles[0];
                FileLogger.Instance.WriteLine($"[LOG] DeferredInit: 选择配置文件 '{_selectedProfileName}'");
                OnPropertyChanged(nameof(SelectedProfileName));
            }
            else
            {
                FileLogger.Instance.WriteLine("[LOG] DeferredInit: 无已保存配置文件，使用 default");
            }

            FileLogger.Instance.WriteLine($"[LOG] DeferredInit: 加载配置文件 '{_selectedProfileName}'...");
            LoadProfile(_selectedProfileName);
            StatusText = "就绪";
            FileLogger.Instance.WriteLine("[LOG] DeferredInit: 完成");
        }
        catch (Exception ex)
        {
            FileLogger.Instance.WriteLine($"[LOG] DeferredInit 异常: {ex.GetType().Name} - {ex.Message}");
            FileLogger.Instance.WriteLine($"[LOG] 堆栈: {ex.StackTrace}");
            StatusText = $"初始化失败: {ex.Message}";
        }
        FileLogger.Instance.WriteLine("[LOG] ===== ContextVM.DeferredInit 结束 =====");
    }

    private ContextAdapterConfig _config = new();
    public ContextAdapterConfig Config { get => _config; set => SetProperty(ref _config, value); }

    public ObservableCollection<ContextStaticItem> StaticValues { get; } = new();
    public ObservableCollection<DynamicContextRule> DynamicRules { get; } = new();
    public ObservableCollection<string> UnconfiguredFields { get; } = new();
    public ObservableCollection<ContextPreviewItem> PreviewItems { get; } = new();
    public List<ContextValueSource> SourceList { get; }

    private List<string> _availableProfiles = new();
    public List<string> AvailableProfiles { get => _availableProfiles; set => SetProperty(ref _availableProfiles, value); }

    private string _selectedProfileName = string.Empty;
    public string SelectedProfileName
    {
        get => _selectedProfileName;
        set
        {
            if (SetProperty(ref _selectedProfileName, value))
            {
                LoadProfile(value);
            }
        }
    }

    private string _statusText = "就绪";
    public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

    public RelayCommand NewProfileCommand { get; }
    public RelayCommand DeleteProfileCommand { get; }
    public RelayCommand AddStaticValueCommand { get; }
    public RelayCommand RemoveStaticValueCommand { get; }
    public RelayCommand AddDynamicRuleCommand { get; }
    public RelayCommand RemoveDynamicRuleCommand { get; }
    public RelayCommand DetectUnconfiguredCommand { get; }
    public RelayCommand PreviewCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand ApplyCommand { get; }

    private void LoadProfile(string profileName)
    {
        if (string.IsNullOrEmpty(profileName)) return;
        Config = _factory.LoadProfile(profileName);
        Config.Type = AdapterType.Context;

        StaticValues.Clear();
        foreach (var kvp in Config.StaticValues)
            StaticValues.Add(new ContextStaticItem { DataPath = kvp.Key, Value = kvp.Value });

        DynamicRules.Clear();
        foreach (var rule in Config.DynamicRules)
            DynamicRules.Add(rule);

        StatusText = $"已加载配置文件: {profileName}";
    }

    private void ExecuteNewProfile()
    {
        var existingCount = AvailableProfiles.Count;
        var newName = $"配置文件 {existingCount + 1}";
        var newConfig = new ContextAdapterConfig
        {
            Type = AdapterType.Context,
            ProfileName = newName
        };
        _factory.SaveProfile(newConfig);
        AvailableProfiles = _factory.GetProfileNames();
        SelectedProfileName = newName;
        StatusText = $"已创建配置文件: {newName}";
    }

    private void ExecuteDeleteProfile()
    {
        if (string.IsNullOrEmpty(SelectedProfileName)) return;
        if (!_dialogService.Confirm($"确定要删除配置文件 \"{SelectedProfileName}\" 吗？", "删除配置文件")) return;

        _factory.DeleteProfile(SelectedProfileName);
        AvailableProfiles = _factory.GetProfileNames();
        if (AvailableProfiles.Count > 0)
            SelectedProfileName = AvailableProfiles[0];
        else
        {
            _selectedProfileName = "default";
            LoadProfile("default");
            OnPropertyChanged(nameof(SelectedProfileName));
        }
        StatusText = "配置文件已删除";
    }

    private void ExecuteAddStaticValue()
    {
        StaticValues.Add(new ContextStaticItem());
    }

    private void ExecuteAddDynamicRule()
    {
        DynamicRules.Add(new DynamicContextRule());
    }

    private void ExecuteDetectUnconfigured()
    {
        SyncConfig();
        var fields = _factory.DetectUnconfiguredFields(_template, Config);
        UnconfiguredFields.Clear();
        foreach (var f in fields) UnconfiguredFields.Add(f);
        StatusText = fields.Count > 0 ? $"发现 {fields.Count} 个未配置字段" : "所有上下文字段均已配置";
    }

    private void ExecutePreview()
    {
        SyncConfig();
        try
        {
            var result = _factory.Fill(_template, Config);
            PreviewItems.Clear();
            if (result.Success)
            {
                foreach (var kvp in result.Data)
                {
                    var source = Config.StaticValues.ContainsKey(kvp.Key) ? "静态值"
                        : Config.DynamicRules.Any(r => r.DataPath == kvp.Key) ? "动态规则"
                        : "内置规则";
                    PreviewItems.Add(new ContextPreviewItem
                    {
                        DataPath = kvp.Key,
                        Value = kvp.Value?.ToString() ?? string.Empty,
                        Source = source
                    });
                }
                StatusText = $"预览完成, {result.Data.Count} 个字段";
            }
            else
            {
                StatusText = $"预览失败: {result.ErrorMessage}";
                _dialogService.ShowError(result.ErrorMessage ?? "预览失败", "错误");
            }
        }
        catch (Exception ex)
        {
            StatusText = $"预览失败: {ex.Message}";
            _dialogService.ShowError($"预览失败: {ex.Message}", "错误");
        }
    }

    private void ExecuteSave()
    {
        SyncConfig();
        try
        {
            _factory.SaveProfile(Config);
            AvailableProfiles = _factory.GetProfileNames();
            StatusText = "配置已保存";
            _dialogService.ShowSuccess("配置已保存");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"保存失败: {ex.Message}", "错误");
        }
    }

    private void ExecuteApply()
    {
        SyncConfig();
        try
        {
            var result = _factory.Fill(_template, Config);
            if (result.Success)
            {
                foreach (var element in _template.Elements)
                {
                    if (!string.IsNullOrEmpty(element.DataPath) && result.Data.TryGetValue(element.DataPath, out var value))
                    {
                        element.DefaultValue = value?.ToString() ?? string.Empty;
                    }
                }
                StatusText = $"已应用上下文填充, {result.Data.Count} 个字段";
                _dialogService.ShowSuccess($"已填充 {result.Data.Count} 个上下文字段");
            }
            else
            {
                _dialogService.ShowError(result.ErrorMessage ?? "应用失败", "错误");
                StatusText = $"应用失败: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"应用失败: {ex.Message}", "错误");
            StatusText = $"应用失败: {ex.Message}";
        }
    }

    private void SyncConfig()
    {
        Config.StaticValues = StaticValues.Where(item => !string.IsNullOrEmpty(item.DataPath))
            .ToDictionary(item => item.DataPath, item => item.Value);
        Config.DynamicRules = DynamicRules.ToList();
    }
}

public class ContextStaticItem : ViewModelBase
{
    public string DataPath { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class ContextPreviewItem : ViewModelBase
{
    public string DataPath { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
}
