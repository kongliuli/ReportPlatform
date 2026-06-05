using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReportDataMaker.Services;
using ReportDataMaker.Services.PdfExport;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.ViewModels.Tabs;

public partial class ExportTabViewModel : MainTabViewModel
{
    [ObservableProperty] private string _outputDirectory = string.Empty;
    [ObservableProperty] private string _fileNamePattern = "{index}";
    [ObservableProperty] private int _exportProgress;
    [ObservableProperty] private bool _isExporting;

    private readonly IDataBindingService _dataBindingService;
    private readonly IPdfExportService _pdfExportService;

    public ExportTabViewModel(MainViewModel mainViewModel, IDataBindingService dataBindingService, IPdfExportService pdfExportService) : base(mainViewModel) { _dataBindingService = dataBindingService; _pdfExportService = pdfExportService; }

    [RelayCommand]
    private async Task ExportSinglePdf()
    {
        if (CurrentTemplate == null) return;
        IsExporting = true;
        try
        {
            var data = _dataBindingService.ExtractData(CurrentTemplate);
            var pdfBytes = await Task.Run(() => _pdfExportService.RenderToPdf(CurrentTemplate, data));
            var filePath = Path.Combine(
                string.IsNullOrEmpty(OutputDirectory) ? Environment.GetFolderPath(Environment.SpecialFolder.Desktop) : OutputDirectory,
                $"{CurrentTemplate.Name ?? "report"}.pdf");
            await File.WriteAllBytesAsync(filePath, pdfBytes);
            StatusText = $"已导出: {filePath}";
        }
        catch (Exception ex)
        {
            StatusText = $"导出失败: {ex.Message}";
        }
        finally { IsExporting = false; }
    }

    [RelayCommand]
    private async Task ExportBatchPdf()
    {
        if (CurrentTemplate == null) return;
        IsExporting = true;
        try
        {
            var batchData = new List<Dictionary<string, object>>
            {
                _dataBindingService.ExtractData(CurrentTemplate)
            };
            var progress = new Progress<int>(p => ExportProgress = p);
            var results = await _pdfExportService.BatchExportAsync(
                CurrentTemplate, batchData,
                string.IsNullOrEmpty(OutputDirectory) ? Environment.GetFolderPath(Environment.SpecialFolder.Desktop) : OutputDirectory,
                FileNamePattern, progress);
            StatusText = $"批量导出完成: {results.Count} 个文件";
        }
        catch (Exception ex)
        {
            StatusText = $"批量导出失败: {ex.Message}";
        }
        finally { IsExporting = false; }
    }
}
