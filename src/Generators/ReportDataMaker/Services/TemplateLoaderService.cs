using System.IO;
using Xinglin.ReportEditor.Contracts;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Services;

public class TemplateLoaderService : ITemplateLoaderService
{
    public TemplateDefinition LoadFromFile(string filePath)
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

    public TemplateDefinition LoadFromJson(string json)
    {
        FileLogger.Instance.WriteLine($"[TemplateLoader] 开始反序列化模板...");
        FileLogger.Instance.WriteLine($"[TemplateLoader] JSON内容长度: {json.Length} 字符");

        try
        {
            FileLogger.Instance.WriteLine($"[TemplateLoader] 调用 TemplateSerializer.Deserialize...");
            var template = TemplateSerializer.Deserialize(json);
            FileLogger.Instance.WriteLine($"[TemplateLoader] TemplateSerializer.Deserialize 调用完成");

            FileLogger.Instance.WriteLine($"[TemplateLoader] ========== 反序列化成功 ==========");
            FileLogger.Instance.WriteLine($"[TemplateLoader] 模板ID: {template.Id ?? "(空)"}");
            FileLogger.Instance.WriteLine($"[TemplateLoader] 模板名称: {template.Name ?? "(空)"}");
            FileLogger.Instance.WriteLine($"[TemplateLoader] 元素数量: {template.Elements?.Count ?? 0}");
            FileLogger.Instance.WriteLine($"[TemplateLoader] 页面宽度: {template.PageSettings.PageWidth}mm");
            FileLogger.Instance.WriteLine($"[TemplateLoader] 页面高度: {template.PageSettings.PageHeight}mm");
            FileLogger.Instance.WriteLine($"[TemplateLoader] 版本: {template.Version}");

            if (template.Elements != null && template.Elements.Any())
            {
                var elementTypes = template.Elements.GroupBy(e => e.GetType().Name).ToDictionary(g => g.Key, g => g.Count());
                FileLogger.Instance.WriteLine($"[TemplateLoader] 元素类型分布 ({elementTypes.Count} 种):");
                foreach (var kvp in elementTypes)
                {
                    FileLogger.Instance.WriteLine($"  - {kvp.Key}: {kvp.Value} 个");
                }

                var nestedCount = template.Elements.OfType<ContainerElement>().Sum(c => c.Children.Count);
                nestedCount += template.Elements.OfType<HeaderElement>().Sum(h => h.Children.Count);
                nestedCount += template.Elements.OfType<FooterElement>().Sum(f => f.Children.Count);
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

    public Task<TemplateDefinition> LoadFromServerAsync(Guid templateId)
    {
        FileLogger.Instance.WriteLine($"[TemplateLoader] LoadFromServerAsync 未实现，templateId: {templateId}");
        throw new NotImplementedException();
    }

    private static void PostProcessElements(List<ExternalElementBase> elements)
    {
        foreach (var element in elements)
        {
            if (element is TableElement table && (table.CellData?.Count ?? 0) > 0)
            {
                table.Group = ElementGroup.Editable;
                if (string.IsNullOrEmpty(table.DataPath))
                {
                    table.DataPath = table.Id;
                }
                FileLogger.Instance.WriteLine($"[TemplateLoader] 表格元素 {table.Id}: 已标记为 Editable，DataPath={table.DataPath}");
            }
        }
    }
}
