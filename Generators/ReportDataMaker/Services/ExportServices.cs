using ReportDataMaker.Services.PdfExport;

namespace ReportDataMaker.Services;

/// <summary>聚合导出相关服务，减少构造器参数</summary>
public class ExportServices
{
    public IPdfExportService PdfExportService { get; }
    public BatchExportService BatchExportService { get; }
    public ExportHistoryStore ExportHistoryStore { get; }

    public ExportServices(IPdfExportService pdfExportService, BatchExportService batchExportService, ExportHistoryStore exportHistoryStore)
    {
        PdfExportService = pdfExportService;
        BatchExportService = batchExportService;
        ExportHistoryStore = exportHistoryStore;
    }
}
