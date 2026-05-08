using System.IO;
using Newtonsoft.Json;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Models;

namespace ReportDataMaker.Services;

public class TemplateLoaderService : ITemplateLoaderService
{
    private readonly IDialogService _dialogService;

    public TemplateLoaderService(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public ExternalTemplateDefinition LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("模板文件不存在", filePath);
        var json = File.ReadAllText(filePath);
        return LoadFromJson(json);
    }

    public ExternalTemplateDefinition LoadFromJson(string json)
    {
        var template = JsonConvert.DeserializeObject<ExternalTemplateDefinition>(json);
        if (template == null)
            throw new InvalidOperationException("无法解析模板 JSON");
        return template;
    }

    public Task<ExternalTemplateDefinition> LoadFromServerAsync(Guid templateId)
    {
        throw new NotImplementedException();
    }
}
