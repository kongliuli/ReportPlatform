using ReportDataMaker.Models;

namespace ReportDataMaker.Services;

public interface ITemplateLoaderService
{
    ExternalTemplateDefinition LoadFromFile(string filePath);
    ExternalTemplateDefinition LoadFromJson(string json);
    Task<ExternalTemplateDefinition> LoadFromServerAsync(Guid templateId);
}
