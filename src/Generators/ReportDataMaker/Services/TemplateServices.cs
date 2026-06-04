using ReportDataMaker.Services.PdfExport;

namespace ReportDataMaker.Services;

/// <summary>聚合模板相关服务，减少构造器参数</summary>
public class TemplateServices
{
    public ITemplateLoaderService TemplateLoader { get; }
    public IDataBindingService DataBindingService { get; }
    public ITemplatePreviewService PreviewService { get; }

    public TemplateServices(ITemplateLoaderService templateLoader, IDataBindingService dataBindingService, ITemplatePreviewService previewService)
    {
        TemplateLoader = templateLoader;
        DataBindingService = dataBindingService;
        PreviewService = previewService;
    }
}
