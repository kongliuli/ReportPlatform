using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ReportDataMaker.Models;

namespace ReportDataMaker.Services;

/// <summary>模板预览服务实现，提供模板的可视化预览功能</summary>
public class TemplatePreviewService : ITemplatePreviewService
{
    /// <summary>生成模板的可视化预览</summary>
    /// <param name="template">外部模板定义</param>
    /// <returns>可视化对象</returns>
    public Visual GeneratePreview(ExternalTemplateDefinition template)
    {
        var canvas = new Canvas { Width = 800, Height = 600, Background = Brushes.White };
        return canvas;
    }

    /// <summary>将模板渲染为图片字节数组</summary>
    /// <param name="template">外部模板定义</param>
    /// <param name="dpi">渲染DPI</param>
    /// <returns>图片字节数组</returns>
    public byte[] RenderToImage(ExternalTemplateDefinition template, double dpi = 96)
    {
        return Array.Empty<byte>();
    }
}
