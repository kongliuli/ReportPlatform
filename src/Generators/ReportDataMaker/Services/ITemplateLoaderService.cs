using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Services;

public interface ITemplateLoaderService
{
    TemplateDefinition LoadFromFile(string filePath);
    TemplateDefinition LoadFromJson(string json);
    Task<TemplateDefinition> LoadFromServerAsync(Guid templateId);
}
