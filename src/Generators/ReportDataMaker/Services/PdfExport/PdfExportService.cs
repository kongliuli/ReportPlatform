using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Services.PdfExport;

public class PdfExportService : IPdfExportService
{
    private readonly PdfElementRenderer _renderer = new();

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

                if (headerElements.Count > 0)
                {
                    page.Header().Canvas((canvas, size) =>
                    {
                        foreach (var element in headerElements)
                            _renderer.RenderElement((SKCanvas)canvas, element, data, layout);
                    });
                }

                if (footerElements.Count > 0)
                {
                    page.Footer().Canvas((canvas, size) =>
                    {
                        foreach (var element in footerElements)
                            _renderer.RenderElement((SKCanvas)canvas, element, data, layout);
                    });
                }

                page.Content().Canvas((canvas, size) =>
                {
                    foreach (var element in contentElements)
                        _renderer.RenderElement((SKCanvas)canvas, element, data, layout);
                });
            });
        });

        return document.GeneratePdf();
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
