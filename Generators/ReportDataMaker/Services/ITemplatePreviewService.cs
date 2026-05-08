using System.Windows.Media;
using ReportDataMaker.Models;

namespace ReportDataMaker.Services;

/// <summary>模板预览服务接口，定义模板可视化预览的契约</summary>
public interface ITemplatePreviewService
{
    /// <summary>生成模板的可视化预览</summary>
    /// <param name="template">外部模板定义</param>
    /// <returns>可视化对象</returns>
    Visual GeneratePreview(ExternalTemplateDefinition template);
    /// <summary>将模板渲染为图片字节数组</summary>
    /// <param name="template">外部模板定义</param>
    /// <param name="dpi">渲染DPI</param>
    /// <returns>图片字节数组</returns>
    byte[] RenderToImage(ExternalTemplateDefinition template, double dpi = 96);
}
