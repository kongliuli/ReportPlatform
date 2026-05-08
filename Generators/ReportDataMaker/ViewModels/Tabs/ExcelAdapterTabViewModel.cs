using System.Collections.ObjectModel;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Models;
using ReportDataMaker.Services.ExcelAdapter;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.ViewModels.Tabs;

/// <summary>Excel适配器标签页视图模型，管理Excel模板的导出、导入和校验</summary>
public class ExcelAdapterTabViewModel : TabViewModelBase
{
    private readonly ExcelAdapterFactory _factory;
    private readonly ExternalTemplateDefinition _template;
    private readonly IDialogService _dialogService;

    /// <summary>初始化Excel适配器标签页视图模型</summary>
    /// <param name="template">外部模板定义</param>
    /// <param name="factory">Excel适配器工厂</param>
    /// <param name="dialogService">对话框服务</param>
    /// <param name="displayName">显示名称</param>
    public ExcelAdapterTabViewModel(
        ExternalTemplateDefinition template,
        ExcelAdapterFactory factory,
        IDialogService dialogService,
        string displayName)
    {
        _template = template;
        _factory = factory;
        _dialogService = dialogService;
        Title = $"Excel: {displayName}";
        IsClosable = true;

        Config = new ExcelAdapterConfig
        {
            Type = AdapterType.Excel,
            DisplayName = displayName,
            Mode = ImportMode.Single
        };

        ExportCommand = new RelayCommand(_ => ExecuteExport());
        ImportCommand = new AsyncRelayCommand(ExecuteImport);
        ValidateCommand = new RelayCommand(_ => ExecuteValidate());
        BrowseFileCommand = new RelayCommand(_ => ExecuteBrowseFile());

        RefreshSchema();
    }

    /// <summary>Excel适配器配置</summary>
    public ExcelAdapterConfig Config { get; }
    /// <summary>字段集合</summary>
    public ObservableCollection<FlatField> Fields { get; } = new();
    /// <summary>校验错误集合</summary>
    public ObservableCollection<ValidationError> ValidationErrors { get; } = new();

    private TemplateFieldSchema? _schema;
    /// <summary>模板字段模式</summary>
    public TemplateFieldSchema? Schema { get => _schema; set => SetProperty(ref _schema, value); }

    private string _importFilePath = string.Empty;
    /// <summary>导入文件路径</summary>
    public string ImportFilePath { get => _importFilePath; set => SetProperty(ref _importFilePath, value); }

    private ImportMode _importMode = ImportMode.Single;
    /// <summary>导入模式</summary>
    public ImportMode ImportModeValue { get => _importMode; set => SetProperty(ref _importMode, value); }

    private int _fieldCount;
    /// <summary>字段数量</summary>
    public int FieldCount { get => _fieldCount; set => SetProperty(ref _fieldCount, value); }

    private int _matchedCount;
    /// <summary>匹配字段数量</summary>
    public int MatchedCount { get => _matchedCount; set => SetProperty(ref _matchedCount, value); }

    private int _dataRowCount;
    /// <summary>数据行数</summary>
    public int DataRowCount { get => _dataRowCount; set => SetProperty(ref _dataRowCount, value); }

    private bool _isValid;
    /// <summary>校验是否通过</summary>
    public bool IsValid { get => _isValid; set => SetProperty(ref _isValid, value); }

    private string _statusText = "就绪";
    /// <summary>状态文本</summary>
    public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

    /// <summary>导出模板命令</summary>
    public RelayCommand ExportCommand { get; }
    /// <summary>导入数据命令</summary>
    public AsyncRelayCommand ImportCommand { get; }
    /// <summary>校验数据命令</summary>
    public RelayCommand ValidateCommand { get; }
    /// <summary>浏览文件命令</summary>
    public RelayCommand BrowseFileCommand { get; }

    private void RefreshSchema()
    {
        Schema = _factory.FlattenTemplate(_template);
        Fields.Clear();
        foreach (var f in Schema.Fields) Fields.Add(f);
        FieldCount = Schema.Fields.Count;
        Config.ExportedSchema.Fields = Schema.Fields;
    }

    private void ExecuteExport()
    {
        var filePath = _dialogService.SaveFile(
            "Excel 文件 (*.xlsx)|*.xlsx", "导出 Excel 模版", $"{_template.Name}_导入模板.xlsx");
        if (string.IsNullOrEmpty(filePath)) return;
        try
        {
            _factory.ExportTemplate(filePath, Schema!);
            _dialogService.ShowSuccess($"模板已导出到: {filePath}");
            StatusText = "模板已导出";
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"导出失败: {ex.Message}", "错误");
        }
    }

    private void ExecuteBrowseFile()
    {
        var filePath = _dialogService.OpenFile("Excel 文件 (*.xlsx)|*.xlsx", "选择导入文件");
        if (!string.IsNullOrEmpty(filePath))
        {
            ImportFilePath = filePath;
            CheckContractMatch();
        }
    }

    private void CheckContractMatch()
    {
        if (string.IsNullOrEmpty(ImportFilePath) || Schema == null) return;
        try
        {
            var result = _factory.ReadData(ImportFilePath, Config.ExportedSchema);
            MatchedCount = result.Data.Count;
            DataRowCount = 1;
            StatusText = $"契约匹配: {MatchedCount}/{FieldCount}";
        }
        catch { StatusText = "契约匹配失败"; }
    }

    private async Task ExecuteImport(object? parameter)
    {
        if (string.IsNullOrEmpty(ImportFilePath)) return;
        StatusText = "导入中...";
        try
        {
            var result = ImportModeValue == ImportMode.Single
                ? _factory.ReadData(ImportFilePath, Config.ExportedSchema)
                : _factory.ReadBatchData(ImportFilePath, Config.ExportedSchema);

            if (!result.Success)
            {
                _dialogService.ShowError(result.ErrorMessage ?? "导入失败", "错误");
                StatusText = "导入失败";
                return;
            }

            DataRowCount = ImportModeValue == ImportMode.Single ? 1 : result.BatchData.Count;
            _dialogService.ShowSuccess($"成功导入 {DataRowCount} 行数据");
            StatusText = $"已导入 {DataRowCount} 行";
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"导入失败: {ex.Message}", "错误");
            StatusText = "导入失败";
        }
    }

    private void ExecuteValidate()
    {
        if (string.IsNullOrEmpty(ImportFilePath) || Schema == null) return;
        try
        {
            var result = _factory.ReadBatchData(ImportFilePath, Config.ExportedSchema);
            if (!result.Success) { _dialogService.ShowError(result.ErrorMessage ?? "读取失败", "错误"); return; }

            var report = _factory.Validate(Schema, result.BatchData);
            ValidationErrors.Clear();
            foreach (var e in report.Errors) ValidationErrors.Add(e);
            IsValid = report.IsValid;
            StatusText = report.IsValid ? "校验通过" : $"发现 {report.ErrorCount} 个错误";
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"校验失败: {ex.Message}", "错误");
        }
    }
}
