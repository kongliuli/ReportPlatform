using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Web.Services;

/// <summary>模板服务接口，提供模板加载和管理功能</summary>
public interface ITemplateService
{
    Task<TemplateDefinition?> GetLatestTemplateAsync();
    Task<TemplateDefinition?> GetTemplateAsync(Guid id);
    Task<List<TemplateDefinition>> GetTemplatesAsync();
}
