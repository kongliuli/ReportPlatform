using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Models;
using ReportDataMaker.Services;
using ReportDataMaker.Services.PdfExport;

namespace ReportDataMaker.ViewModels.Tabs;

public class ExportTabViewModel : TabViewModelBase
{
    private readonly ExternalTemplateDefinition _template;
    private readonly IPdfExportService _pdfExportService;
    private readonly BatchExportService _batchExportService;
    private readonly ExportHistoryStore _historyStore;
    private readonly IDialogService _dialogService;
    private readonly DataBindingService _dataBindingService;

    public ExportTabViewModel(
        ExternalTemplateDefinition template,
        IPdfExportService pdfExportService,
        BatchExportService batchExportService,
        ExportHistoryStore historyStore,
        IDialogService dialogService,
        DataBindingService dataBindingService)
    {
        _template = template;
        _pdfExportService = pdfExportService;
        _batchExportService = batchExportService;
        _historyStore = historyStore;
        _dialogService = dialogService;
        _dataBindingService = dataBindingService;
        Title = "导出";
        IsClosable = false;

        DataSourceNames = new ObservableCollection<string> { "当前录入数据", "Excel适配器", "数据库适配器" };
        _selectedDataSource = "当前录入数据";
        _fileNamePattern = "{index}_{date}";

        ExportPdfCommand = new RelayCommand(_ => ExecuteExportPdf());
        BatchExportCommand = new AsyncRelayCommand(ExecuteBatchExport);
        BrowseOutputDirCommand = new RelayCommand(_ => ExecuteBrowseOutputDir());
        PrintPreviewCommand = new RelayCommand(_ => ExecutePrintPreview());
        RefreshHistoryCommand = new RelayCommand(_ => ExecuteRefreshHistory());

        ExecuteRefreshHistory();
    }

    public ExternalTemplateDefinition Template => _template;

    public ObservableCollection<ExportRecord> ExportHistory { get; } = new();
    public ObservableCollection<string> DataSourceNames { get; }

    private string _selectedDataSource;
    public string SelectedDataSource { get => _selectedDataSource; set => SetProperty(ref _selectedDataSource, value); }

    private string _outputDirectory = string.Empty;
    public string OutputDirectory { get => _outputDirectory; set => SetProperty(ref _outputDirectory, value); }

    private string _fileNamePattern;
    public string FileNamePattern { get => _fileNamePattern; set => SetProperty(ref _fileNamePattern, value); }

    private int _progressValue;
    public int ProgressValue { get => _progressValue; set => SetProperty(ref _progressValue, value); }

    private bool _isExporting;
    public bool IsExporting { get => _isExporting; set => SetProperty(ref _isExporting, value); }

    private string _statusText = "就绪";
    public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

    private string _previewInfo = string.Empty;
    public string PreviewInfo { get => _previewInfo; set => SetProperty(ref _previewInfo, value); }

    public RelayCommand ExportPdfCommand { get; }
    public AsyncRelayCommand BatchExportCommand { get; }
    public RelayCommand BrowseOutputDirCommand { get; }
    public RelayCommand PrintPreviewCommand { get; }
    public RelayCommand RefreshHistoryCommand { get; }

    private void ExecuteExportPdf()
    {
        var filePath = _dialogService.SaveFile(
            "PDF 文件 (*.pdf)|*.pdf", "导出 PDF", $"{_template.Name}.pdf");
        if (string.IsNullOrEmpty(filePath)) return;

        try
        {
            var data = _dataBindingService.ExtractData(_template);
            var pdfBytes = _pdfExportService.RenderToPdf(_template, data);
            File.WriteAllBytes(filePath, pdfBytes);

            _historyStore.Add(new ExportRecord
            {
                ExportTime = DateTime.Now,
                FileName = Path.GetFileName(filePath),
                FilePath = filePath,
                TemplateName = _template.Name,
                RecordCount = 1
            });

            StatusText = "PDF 导出成功";
            _dialogService.ShowSuccess($"PDF 已导出到: {filePath}");
            ExecuteRefreshHistory();
        }
        catch (Exception ex)
        {
            StatusText = $"导出失败: {ex.Message}";
            _dialogService.ShowError($"导出失败: {ex.Message}", "错误");
        }
    }

    private async Task ExecuteBatchExport(object? parameter)
    {
        if (string.IsNullOrWhiteSpace(OutputDirectory))
        {
            _dialogService.ShowError("请先设置输出目录", "错误");
            return;
        }
        if (string.IsNullOrWhiteSpace(FileNamePattern))
        {
            _dialogService.ShowError("请先设置文件命名模式", "错误");
            return;
        }

        IsExporting = true;
        ProgressValue = 0;
        StatusText = "正在批量导出...";

        try
        {
            var data = _dataBindingService.ExtractData(_template);
            var batchData = new List<Dictionary<string, object>> { data };

            var options = new BatchExportOptions
            {
                OutputDirectory = OutputDirectory,
                FileNamePattern = FileNamePattern
            };

            var progress = new Progress<int>(p => ProgressValue = p);
            var result = await _batchExportService.ExportAsync(_template, batchData, options, progress);

            if (result.Success)
            {
                StatusText = $"批量导出完成: 成功 {result.SuccessCount} 个";
                _dialogService.ShowSuccess($"批量导出完成，共 {result.SuccessCount} 个文件");

                _historyStore.Add(new ExportRecord
                {
                    ExportTime = DateTime.Now,
                    FileName = $"批量导出_{DateTime.Now:yyyyMMdd}",
                    FilePath = OutputDirectory,
                    TemplateName = _template.Name,
                    RecordCount = result.SuccessCount
                });
            }
            else
            {
                StatusText = $"批量导出完成: 成功 {result.SuccessCount}, 失败 {result.FailCount}";
                _dialogService.ShowError($"批量导出完成，成功 {result.SuccessCount} 个，失败 {result.FailCount} 个", "导出结果");
            }

            ExecuteRefreshHistory();
        }
        catch (Exception ex)
        {
            StatusText = $"批量导出失败: {ex.Message}";
            _dialogService.ShowError($"批量导出失败: {ex.Message}", "错误");
        }
        finally
        {
            IsExporting = false;
            ProgressValue = 100;
        }
    }

    private void ExecuteBrowseOutputDir()
    {
        var filePath = _dialogService.OpenFile("所有文件 (*.*)|*.*", "选择输出目录中的任意文件");
        if (!string.IsNullOrEmpty(filePath))
        {
            OutputDirectory = Path.GetDirectoryName(filePath) ?? string.Empty;
        }
    }

    private void ExecutePrintPreview()
    {
        _dialogService.ShowInfo("打印预览功能尚未实现", "提示");
    }

    private void ExecuteRefreshHistory()
    {
        ExportHistory.Clear();
        foreach (var record in _historyStore.GetAll())
        {
            ExportHistory.Add(record);
        }
        StatusText = $"已加载 {ExportHistory.Count} 条导出记录";
    }
}
