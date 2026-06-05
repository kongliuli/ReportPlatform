namespace ReportDataMaker.Web.Services;

/// <summary>PDF服务接口，提供PDF渲染功能</summary>
public interface IPdfService
{
    Task<byte[]?> RenderPdfAsync(Guid templateId, Dictionary<string, object> data);
}
