using System.Collections.ObjectModel;
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

    public ExportTabViewModel(MainViewModel mainViewModel) : base(mainViewModel) { }

    [RelayCommand]
    private async Task ExportSinglePdf()
    {
        if (CurrentTemplate == null) return;
        IsExporting = true;
        try
        {
            var data = MainViewModel._dataBindingService.ExtractData(CurrentTemplate);
            var pdfBytes = await Task.Run(() => MainViewModel._pdfExportService.RenderToPdf(CurrentTemplate, data));
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
                MainViewModel._dataBindingService.ExtractData(CurrentTemplate)
            };
            var progress = new Progress<int>(p => ExportProgress = p);
            var results = await MainViewModel._pdfExportService.BatchExportAsync(
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
