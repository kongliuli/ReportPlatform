using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Services.PdfExport;

/// <summary>导出适配器插件</summary>
public class ExportAdapterPlugin : IAdapterPlugin
{
    private readonly IPdfExportService _pdfExportService;

    public ExportAdapterPlugin(IPdfExportService pdfExportService)
    {
        _pdfExportService = pdfExportService;
    }

    public string AdapterType => "export";
    public string DisplayName => "PDF 导出适配器";

    public object CreateService(TemplateDefinition template)
    {
        return _pdfExportService;
    }
}
