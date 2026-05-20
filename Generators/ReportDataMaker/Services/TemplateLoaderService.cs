using System.IO;
using Newtonsoft.Json;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Models;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.Services;

/// <summary>模板加载服务实现，提供从文件和JSON加载模板的功能</summary>
public class TemplateLoaderService : ITemplateLoaderService
{
    private readonly IDialogService _dialogService;
    private readonly JsonSerializerSettings _serializerSettings;

    /// <summary>初始化加载服务</summary>
    /// <param name="dialogService">对话框服务</param>
    public TemplateLoaderService(IDialogService dialogService)
    {
        FileLogger.Instance.WriteLine($"[TemplateLoaderService] 初始化模板加载服务");
        _dialogService = dialogService;
        _serializerSettings = new JsonSerializerSettings
        {
            Converters = { new ReportExternalElementConverter() }
        };
        FileLogger.Instance.WriteLine($"[TemplateLoaderService] 序列化设置已配置，转换器已注册");
    }

    /// <summary>从文件加载模板定义</summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>外部模板定义</returns>
    public ExternalTemplateDefinition LoadFromFile(string filePath)
    {
        FileLogger.Instance.WriteLine($"[TemplateLoader] ========== 开始加载模板文件 ==========");
        FileLogger.Instance.WriteLine($"[TemplateLoader] 文件路径: {filePath}");
        
        if (!File.Exists(filePath))
        {
            FileLogger.Instance.WriteLine($"[TemplateLoader] 错误: 文件不存在 - {filePath}");
            throw new FileNotFoundException("模板文件不存在", filePath);
        }
        
        FileLogger.Instance.WriteLine($"[TemplateLoader] 文件存在，开始读取内容...");
        var json = File.ReadAllText(filePath);
        FileLogger.Instance.WriteLine($"[TemplateLoader] 文件读取完成，大小: {json.Length} 字节");
        
        return LoadFromJson(json);
    }

    /// <summary>从JSON字符串加载模板定义</summary>
    /// <param name="json">JSON字符串</param>
    /// <returns>外部模板定义</returns>
    public ExternalTemplateDefinition LoadFromJson(string json)
    {
        FileLogger.Instance.WriteLine($"[TemplateLoader] 开始反序列化模板...");
        FileLogger.Instance.WriteLine($"[TemplateLoader] JSON内容长度: {json.Length} 字符");
        
        try
        {
            FileLogger.Instance.WriteLine($"[TemplateLoader] 调用 JsonConvert.DeserializeObject...");
            var template = JsonConvert.DeserializeObject<ExternalTemplateDefinition>(json, _serializerSettings);
            FileLogger.Instance.WriteLine($"[TemplateLoader] JsonConvert.DeserializeObject 调用完成");
            
            if (template == null)
            {
                FileLogger.Instance.WriteLine($"[TemplateLoader] 错误: 反序列化结果为null");
                throw new InvalidOperationException("无法解析模板 JSON");
            }
            
            FileLogger.Instance.WriteLine($"[TemplateLoader] ========== 反序列化成功 ==========");
            FileLogger.Instance.WriteLine($"[TemplateLoader] 模板ID: {template.Id ?? "(空)"}");
            FileLogger.Instance.WriteLine($"[TemplateLoader] 模板名称: {template.Name ?? "(空)"}");
            FileLogger.Instance.WriteLine($"[TemplateLoader] 元素数量: {template.Elements?.Count ?? 0}");
            FileLogger.Instance.WriteLine($"[TemplateLoader] 页面宽度: {template.PageWidth}mm");
            FileLogger.Instance.WriteLine($"[TemplateLoader] 页面高度: {template.PageHeight}mm");
            FileLogger.Instance.WriteLine($"[TemplateLoader] 版本: {template.Version ?? "(空)"}");
            
            if (template.Elements != null && template.Elements.Any())
            {
                var elementTypes = template.Elements.GroupBy(e => e.GetType().Name).ToDictionary(g => g.Key, g => g.Count());
                FileLogger.Instance.WriteLine($"[TemplateLoader] 元素类型分布 ({elementTypes.Count} 种):");
                foreach (var kvp in elementTypes)
                {
                    FileLogger.Instance.WriteLine($"  - {kvp.Key}: {kvp.Value} 个");
                }
                
                var nestedCount = template.Elements.OfType<ExternalContainerElement>().Sum(c => c.Children.Count);
                nestedCount += template.Elements.OfType<ExternalHeaderElement>().Sum(h => h.Children.Count);
                nestedCount += template.Elements.OfType<ExternalFooterElement>().Sum(f => f.Children.Count);
                FileLogger.Instance.WriteLine($"[TemplateLoader] 嵌套子元素数量: {nestedCount}");

                PostProcessElements(template.Elements);
            }
            else
            {
                FileLogger.Instance.WriteLine($"[TemplateLoader] 元素列表为空或null");
            }
            
            FileLogger.Instance.WriteLine($"[TemplateLoader] ========== 模板加载完成 ==========");
            return template;
        }
        catch (Exception ex)
        {
            FileLogger.Instance.WriteLine($"[TemplateLoader] ========== 反序列化失败 ==========");
            FileLogger.Instance.WriteLine($"[TemplateLoader] 异常类型: {ex.GetType().Name}");
            FileLogger.Instance.WriteLine($"[TemplateLoader] 异常消息: {ex.Message}");
            FileLogger.Instance.WriteLine($"[TemplateLoader] 异常堆栈:\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                FileLogger.Instance.WriteLine($"[TemplateLoader] 内部异常类型: {ex.InnerException.GetType().Name}");
                FileLogger.Instance.WriteLine($"[TemplateLoader] 内部异常消息: {ex.InnerException.Message}");
            }
            FileLogger.Instance.WriteLine($"[TemplateLoader] ========== 异常结束 ==========");
            throw;
        }
    }

    /// <summary>从服务器异步加载模板定义</summary>
    /// <param name="templateId">模板标识</param>
    /// <returns>外部模板定义</returns>
    public Task<ExternalTemplateDefinition> LoadFromServerAsync(Guid templateId)
    {
        FileLogger.Instance.WriteLine($"[TemplateLoader] LoadFromServerAsync 未实现，templateId: {templateId}");
        throw new NotImplementedException();
    }

    /// <summary>后处理元素列表：修正自动分类不准确的情况</summary>
    private static void PostProcessElements(List<ReportExternalElementBase> elements)
    {
        foreach (var element in elements)
        {
            // 表格元素：如果包含 CellData（即有可填充数据），应标记为 Editable
            if (element is ExternalTableElement table && table.CellData.Count > 0)
            {
                table.Group = ElementGroup.Editable;
                // 如果没有 DataPath，自动使用 Id 作为 DataPath
                if (string.IsNullOrEmpty(table.DataPath))
                {
                    table.DataPath = table.Id;
                }
                FileLogger.Instance.WriteLine($"[TemplateLoader] 表格元素 {table.Id}: 已标记为 Editable，DataPath={table.DataPath}");
            }
        }
    }
}
