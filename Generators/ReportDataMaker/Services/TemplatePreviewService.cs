using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Models;

namespace ReportDataMaker.Services;

public class TemplatePreviewService : ITemplatePreviewService
{
    private readonly CanvasRenderer _canvasRenderer = new();

    public Visual GeneratePreview(ExternalTemplateDefinition template)
    {
        if (template?.Elements == null)
            return new TextBlock { Text = "无模板数据", Foreground = Brushes.Gray, Margin = new Thickness(16) };

        var canvas = new Canvas
        {
            Background = Brushes.White,
            UseLayoutRounding = true,
            SnapsToDevicePixels = true
        };

        _canvasRenderer.RenderToCanvas(canvas, template);
        return canvas;
    }

    public byte[] RenderToImage(ExternalTemplateDefinition template, double dpi = 96)
    {
        // Keep SkiaSharp path for image export (PDF/PNG)
        var layout = new PdfExport.PdfPageLayoutEngine(template);
        var scale = dpi / 96;
        var width = (int)(layout.PageWidth * scale);
        var height = (int)(layout.PageHeight * scale);

        using var bitmap = new SkiaSharp.SKBitmap(width, height);
        using var skCanvas = new SkiaSharp.SKCanvas(bitmap);
        skCanvas.Clear(SkiaSharp.SKColors.White);

        var scaledLayout = new PdfExport.PdfPageLayoutEngine(template, scale);
        var renderer = new PdfExport.PdfElementRenderer();
        var data = new System.Collections.Generic.Dictionary<string, object>();

        foreach (var element in template.Elements.OrderBy(e => e.ZIndex))
            renderer.RenderElement(skCanvas, element, data, scaledLayout);

        using var image = bitmap.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
        return image.ToArray();
    }
}