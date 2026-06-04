using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Services;

public class TemplatePreviewService : ITemplatePreviewService
{
    private readonly CanvasRenderer _canvasRenderer = new();

    public Visual GeneratePreview(TemplateDefinition template)
    {
        var canvas = new System.Windows.Controls.Canvas
        {
            Width = template.PageSettings.PageWidth * 3.7795275591,
            Height = template.PageSettings.PageHeight * 3.7795275591
        };

        _canvasRenderer.RenderToCanvas(canvas, template);
        return canvas;
    }

    public byte[] RenderToImage(TemplateDefinition template, double dpi = 96)
    {
        var visual = GeneratePreview(template);
        var bounds = new Rect(new Size(
            template.PageSettings.PageWidth * 3.7795275591,
            template.PageSettings.PageHeight * 3.7795275591));

        var renderTarget = new RenderTargetBitmap(
            (int)(bounds.Width * dpi / 96),
            (int)(bounds.Height * dpi / 96),
            dpi, dpi, PixelFormats.Pbgra32);

        renderTarget.Render(visual);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(renderTarget));

        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }
}
