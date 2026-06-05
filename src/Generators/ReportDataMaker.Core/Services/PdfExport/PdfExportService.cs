using System.IO;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;
using Xinglin.ReportEditor.Contracts.Models.Template;
using Xinglin.ReportEditor.Rendering.Services;

namespace ReportDataMaker.Services.PdfExport;

public class PdfExportService : IPdfExportService, IDataAdapter
{
    private readonly TemplateRenderer _renderer = new();

    public string AdapterId => "pdf-export";
    public string AdapterName => "PDF 导出适配器";
    public AdapterType Type => AdapterType.Api;
    public IReadOnlyList<string> TargetDataPaths => Array.Empty<string>();

    /// <summary>导出服务不实现数据读取</summary>
    public Task<AdapterResult> ReadDataAsync() => throw new NotImplementedException();
    /// <summary>导出服务不实现数据读取</summary>
    public Task<AdapterResult> ReadBatchDataAsync() => throw new NotImplementedException();
    public Task<ValidationResult> ValidateConfigAsync() => Task.FromResult(ValidationResult.Success);

    public byte[] RenderToPdf(TemplateDefinition template, Dictionary<string, object> data)
    {
        return _renderer.RenderToPdf(template, data);
    }

    public async Task<List<string>> BatchExportAsync(
        TemplateDefinition template,
        List<Dictionary<string, object>> batchData,
        string outputDirectory,
        string fileNamePattern,
        IProgress<int>? progress = null)
    {
        var outputFiles = new List<string>();
        Directory.CreateDirectory(outputDirectory);

        for (int i = 0; i < batchData.Count; i++)
        {
            var fileName = fileNamePattern
                .Replace("{index}", (i + 1).ToString())
                .Replace("{date}", DateTime.Now.ToString("yyyyMMdd"))
                .Replace("{name}", template.Name);

            var pdfBytes = RenderToPdf(template, batchData[i]);
            var filePath = Path.Combine(outputDirectory, $"{fileName}.pdf");
            await File.WriteAllBytesAsync(filePath, pdfBytes);
            outputFiles.Add(filePath);

            progress?.Report((i + 1) * 100 / batchData.Count);
        }

        return outputFiles;
    }
}
