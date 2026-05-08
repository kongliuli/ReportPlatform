using ReportDataMaker.Models;

namespace ReportDataMaker.Services;

/// <summary>模板加载服务接口，定义模板加载的契约</summary>
public interface ITemplateLoaderService
{
    /// <summary>从文件加载模板定义</summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>外部模板定义</returns>
    ExternalTemplateDefinition LoadFromFile(string filePath);
    /// <summary>从JSON字符串加载模板定义</summary>
    /// <param name="json">JSON字符串</param>
    /// <returns>外部模板定义</returns>
    ExternalTemplateDefinition LoadFromJson(string json);
    /// <summary>从服务器异步加载模板定义</summary>
    /// <param name="templateId">模板标识</param>
    /// <returns>外部模板定义</returns>
    Task<ExternalTemplateDefinition> LoadFromServerAsync(Guid templateId);
}
