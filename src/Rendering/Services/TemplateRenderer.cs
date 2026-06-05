using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using Xinglin.ReportEditor.Contracts;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace Xinglin.ReportEditor.Rendering.Services;

public class TemplateRenderer
{
    private readonly PdfElementRenderer _elementRenderer = new();

    public byte[] RenderToPdf(TemplateDefinition template)
    {
        return RenderToPdf(template, new Dictionary<string, object>());
    }

    public byte[] RenderToPdf(TemplateDefinition template, Dictionary<string, object> data)
    {
        var layout = new PdfPageLayoutEngine(template);
        var contentWidth = layout.PageWidth - layout.MarginLeft - layout.MarginRight;

        var headerElements = template.Elements
            .Where(e => e.IsVisible && e is HeaderElement)
            .OrderBy(e => e.ZIndex)
            .ToList();

        var footerElements = template.Elements
            .Where(e => e.IsVisible && e is FooterElement)
            .OrderBy(e => e.ZIndex)
            .ToList();

        var contentElements = template.Elements
            .Where(e => e.IsVisible && e is not HeaderElement && e is not FooterElement)
            .OrderBy(e => e.ZIndex)
            .ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(new PageSize(layout.PageWidth, layout.PageHeight));
                page.MarginLeft(layout.MarginLeft);
                page.MarginRight(layout.MarginRight);
                page.MarginTop(layout.MarginTop);
                page.MarginBottom(layout.MarginBottom);

                if (headerElements.Count > 0)
                {
                    var headerHeight = headerElements.Max(e => layout.ConvertY(e.Y) + layout.ConvertSize(e.Height));
                    page.Header().Element(c =>
                    {
                        var imageBytes = RenderToImage(canvas =>
                        {
                            foreach (var el in headerElements)
                                _elementRenderer.RenderElement(canvas, el, data, layout);
                        }, contentWidth, headerHeight);
                        c.Image(imageBytes);
                    });
                }

                if (footerElements.Count > 0)
                {
                    var footerHeight = footerElements.Max(e => layout.ConvertY(e.Y) + layout.ConvertSize(e.Height));
                    page.Footer().Element(c =>
                    {
                        var imageBytes = RenderToImage(canvas =>
                        {
                            foreach (var el in footerElements)
                                _elementRenderer.RenderElement(canvas, el, data, layout);
                        }, contentWidth, footerHeight);
                        c.Image(imageBytes);
                    });
                }

                page.Content().Element(c =>
                {
                    var contentHeight = layout.PageHeight - layout.MarginTop - layout.MarginBottom;
                    var imageBytes = RenderToImage(canvas =>
                    {
                        foreach (var el in contentElements)
                            _elementRenderer.RenderElement(canvas, el, data, layout);
                    }, contentWidth, contentHeight);
                    c.Image(imageBytes);
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] RenderToPdf(string templateJson)
    {
        var template = TemplateSerializer.Deserialize(templateJson);
        return RenderToPdf(template);
    }

    public byte[] RenderToImage(string templateJson)
    {
        var template = TemplateSerializer.Deserialize(templateJson);
        return RenderToImage(template);
    }

    public byte[] RenderToImage(TemplateDefinition template)
    {
        var layout = new PdfPageLayoutEngine(template);
        var contentWidth = layout.PageWidth - layout.MarginLeft - layout.MarginRight;
        var contentHeight = layout.PageHeight - layout.MarginTop - layout.MarginBottom;

        var allElements = template.Elements
            .Where(e => e.IsVisible)
            .OrderBy(e => e.ZIndex)
            .ToList();

        return RenderToImage(canvas =>
        {
            foreach (var el in allElements)
                _elementRenderer.RenderElement(canvas, el, new Dictionary<string, object>(), layout);
        }, contentWidth, contentHeight);
    }

    private static byte[] RenderToImage(Action<SKCanvas> draw, float width, float height)
    {
        using var surface = SKSurface.Create(new SKImageInfo(Math.Max(1, (int)width), Math.Max(1, (int)height)));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);
        draw(canvas);
        using var image = surface.Snapshot();
        using var imageData = image.Encode(SKEncodedImageFormat.Png, 100);
        return imageData.ToArray();
    }
}
