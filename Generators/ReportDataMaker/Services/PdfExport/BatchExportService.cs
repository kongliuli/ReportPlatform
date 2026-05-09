using System.IO;
using ReportDataMaker.Models;

namespace ReportDataMaker.Services.PdfExport;

public class BatchExportOptions
{
    public string OutputDirectory { get; set; } = string.Empty;
    public string FileNamePattern { get; set; } = "{index}";
    public int Parallelism { get; set; } = 1;
}

public class BatchExportResult
{
    public bool Success { get; set; }
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailCount { get; set; }
    public List<string> OutputFiles { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public class BatchExportService
{
    private readonly IPdfExportService _pdfExportService;
    private readonly object _lock = new();

    public BatchExportService(IPdfExportService pdfExportService)
    {
        _pdfExportService = pdfExportService;
    }

    public async Task<BatchExportResult> ExportAsync(
        ExternalTemplateDefinition template,
        List<Dictionary<string, object>> batchData,
        BatchExportOptions options,
        IProgress<int>? progress = null)
    {
        var result = new BatchExportResult { TotalCount = batchData.Count };
        Directory.CreateDirectory(options.OutputDirectory);

        if (options.Parallelism <= 1)
        {
            for (int i = 0; i < batchData.Count; i++)
            {
                await ProcessSingleAsync(template, batchData[i], i, options, result);
                progress?.Report((i + 1) * 100 / batchData.Count);
            }
        }
        else
        {
            var semaphore = new SemaphoreSlim(options.Parallelism);
            var completed = 0;
            var tasks = batchData.Select(async (dataRow, i) =>
            {
                await semaphore.WaitAsync();
                try
                {
                    await ProcessSingleAsync(template, dataRow, i, options, result);
                }
                finally
                {
                    semaphore.Release();
                    var done = Interlocked.Increment(ref completed);
                    progress?.Report(done * 100 / batchData.Count);
                }
            });
            await Task.WhenAll(tasks);
        }

        result.Success = result.FailCount == 0;
        return result;
    }

    private async Task ProcessSingleAsync(
        ExternalTemplateDefinition template,
        Dictionary<string, object> data,
        int index,
        BatchExportOptions options,
        BatchExportResult result)
    {
        try
        {
            var fileName = options.FileNamePattern
                .Replace("{index}", (index + 1).ToString())
                .Replace("{date}", DateTime.Now.ToString("yyyyMMdd"))
                .Replace("{name}", template.Name);

            var pdfBytes = _pdfExportService.RenderToPdf(template, data);
            var filePath = Path.Combine(options.OutputDirectory, $"{fileName}.pdf");
            await File.WriteAllBytesAsync(filePath, pdfBytes);

            lock (_lock)
            {
                result.OutputFiles.Add(filePath);
                result.SuccessCount++;
            }
        }
        catch (Exception ex)
        {
            lock (_lock)
            {
                result.Errors.Add($"Row {index + 1}: {ex.Message}");
                result.FailCount++;
            }
        }
    }
}
