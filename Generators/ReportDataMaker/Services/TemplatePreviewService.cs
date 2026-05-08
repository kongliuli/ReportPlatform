using System.Windows;
using System.Windows.Media;
using ReportDataMaker.Models;

namespace ReportDataMaker.Services;

public class TemplatePreviewService : ITemplatePreviewService
{
    public Visual GeneratePreview(ExternalTemplateDefinition template)
    {
        var canvas = new Canvas { Width = 800, Height = 600, Background = Brushes.White };
        return canvas;
    }

    public byte[] RenderToImage(ExternalTemplateDefinition template, double dpi = 96)
    {
        return Array.Empty<byte>();
    }
}
