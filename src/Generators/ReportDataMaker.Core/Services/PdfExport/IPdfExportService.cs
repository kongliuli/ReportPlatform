using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Services.PdfExport;

public interface IPdfExportService
{
    byte[] RenderToPdf(TemplateDefinition template, Dictionary<string, object> data);
    Task<List<string>> BatchExportAsync(TemplateDefinition template, List<Dictionary<string, object>> batchData, string outputDirectory, string fileNamePattern, IProgress<int>? progress = null);
}
