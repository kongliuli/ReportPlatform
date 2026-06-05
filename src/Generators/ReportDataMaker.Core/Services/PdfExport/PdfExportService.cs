using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Models.Template;
using Xinglin.ReportEditor.Rendering.Services;

namespace ReportDataMaker.Services.PdfExport;

public class PdfExportService : IPdfExportService, IDataAdapter
{
    private readonly PdfElementRenderer _renderer = new();

    public string AdapterId => "pdf-export";
    public string AdapterName => "PDF 导出适配器";
    public AdapterType Type => AdapterType.Api;
    public IReadOnlyList<string> TargetDataPaths => Array.Empty<string>();      

    public Task<AdapterResult> ReadDataAsync() => throw new NotImplementedException();
    public Task<AdapterResult> ReadBatchDataAsync() => throw new NotImplementedException();
    public Task<ValidationResult> ValidateConfigAsync() => Task.FromResult(ValidationResult.Success);

    public byte[] RenderToPdf(TemplateDefinition template, Dictionary<string, object> data)
    {
        var layout = new PdfPageLayoutEngine(template);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(new PageSize(layout.PageWidth, layout.PageHeight));   
                page.MarginLeft(layout.MarginLeft);
                page.MarginRight(layout.MarginRight);
                page.MarginTop(layout.MarginTop);
                page.MarginBottom(layout.MarginBottom);

                var headerElements = template.Elements
                    .Where(e => e is HeaderElement)
                    .OrderBy(e => e.ZIndex)
                    .ToList();

                var footerElements = template.Elements
                    .Where(e => e is FooterElement)
                    .OrderBy(e => e.ZIndex)
                    .ToList();

                var contentElements = template.Elements
                    .Where(e => e is not HeaderElement && e is not FooterElement)
                    .OrderBy(e => e.ZIndex)
                    .ToList();

                var contentWidth = layout.PageWidth - layout.MarginLeft - layout.MarginRight;
                var contentHeight = layout.PageHeight - layout.MarginTop - layout.MarginBottom;

                if (headerElements.Count > 0)
                {
                    var headerHeight = headerElements.Max(e => layout.ConvertY(e.Y) + layout.ConvertSize(e.Height));
                    page.Header().Element(container =>
                    {
                        var imageBytes = RenderToImage(canvas =>
                        {
                            foreach (var element in headerElements)
                                _renderer.RenderElement(canvas, element, data, layout);
                        }, contentWidth, headerHeight);
                        container.Image(imageBytes);
                    });
                }

                if (footerElements.Count > 0)
                {
                    var footerHeight = footerElements.Max(e => layout.ConvertY(e.Y) + layout.ConvertSize(e.Height));
                    page.Footer().Element(container =>
                    {
                        var imageBytes = RenderToImage(canvas =>
                        {
                            foreach (var element in footerElements)
                                _renderer.RenderElement(canvas, element, data, layout);
                        }, contentWidth, footerHeight);
                        container.Image(imageBytes);
                    });
                }

                page.Content().Element(container =>
                {
                    var imageBytes = RenderToImage(canvas =>
                    {
                        foreach (var element in contentElements)
                            _renderer.RenderElement(canvas, element, data, layout);
                    }, contentWidth, contentHeight);
                    container.Image(imageBytes);
                });
            });
        });

        return document.GeneratePdf();
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

    public async Task<List<string>> BatchExportAsync(
        TemplateDefinition template,
        List<Dictionary<string, object>> batchData,
        string outputDirectory,
        string fileNamePattern,
        IProgress<int>? progress = null)
    {
        var outputFiles = new List<string>();
        Directory.CreateDirectory(outputDirectory);

        for (int i = 0; i < batchData.Count; i++)
        {
            var fileName = fileNamePattern
                .Replace("{index}", (i + 1).ToString())
                .Replace("{date}", DateTime.Now.ToString("yyyyMMdd"))
                .Replace("{name}", template.Name);

            var pdfBytes = RenderToPdf(template, batchData[i]);
            var filePath = Path.Combine(outputDirectory, $"{fileName}.pdf");    
            await File.WriteAllBytesAsync(filePath, pdfBytes);
            outputFiles.Add(filePath);

            progress?.Report((i + 1) * 100 / batchData.Count);
        }

        return outputFiles;
    }
}
