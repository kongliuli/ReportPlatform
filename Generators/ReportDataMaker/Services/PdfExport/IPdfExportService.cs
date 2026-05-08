using ReportDataMaker.Models;

namespace ReportDataMaker.Services.PdfExport;

public interface IPdfExportService
{
    byte[] RenderToPdf(ExternalTemplateDefinition template, Dictionary<string, object> data);
    Task<List<string>> BatchExportAsync(ExternalTemplateDefinition template, List<Dictionary<string, object>> batchData, string outputDirectory, string fileNamePattern, IProgress<int>? progress = null);
}
