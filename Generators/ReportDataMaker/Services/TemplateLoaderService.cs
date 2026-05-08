using System.IO;
using Newtonsoft.Json;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Models;

namespace ReportDataMaker.Services;

/// <summary>模板加载服务实现，提供从文件和JSON加载模板的功能</summary>
public class TemplateLoaderService : ITemplateLoaderService
{
    private readonly IDialogService _dialogService;

    /// <summary>初始化模板加载服务</summary>
    /// <param name="dialogService">对话框服务</param>
    public TemplateLoaderService(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    /// <summary>从文件加载模板定义</summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>外部模板定义</returns>
    public ExternalTemplateDefinition LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("模板文件不存在", filePath);
        var json = File.ReadAllText(filePath);
        return LoadFromJson(json);
    }

    /// <summary>从JSON字符串加载模板定义</summary>
    /// <param name="json">JSON字符串</param>
    /// <returns>外部模板定义</returns>
    public ExternalTemplateDefinition LoadFromJson(string json)
    {
        var template = JsonConvert.DeserializeObject<ExternalTemplateDefinition>(json);
        if (template == null)
            throw new InvalidOperationException("无法解析模板 JSON");
        return template;
    }

    /// <summary>从服务器异步加载模板定义</summary>
    /// <param name="templateId">模板标识</param>
    /// <returns>外部模板定义</returns>
    public Task<ExternalTemplateDefinition> LoadFromServerAsync(Guid templateId)
    {
        throw new NotImplementedException();
    }
}
