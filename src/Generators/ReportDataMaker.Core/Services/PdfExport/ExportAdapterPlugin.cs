using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;
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

    public AdapterType AdapterType => AdapterType.Export;
    public string DisplayName => "PDF 导出适配器";

    public IDataAdapter CreateService(TemplateDefinition template)
    {
        return (IDataAdapter)_pdfExportService;
    }
}
