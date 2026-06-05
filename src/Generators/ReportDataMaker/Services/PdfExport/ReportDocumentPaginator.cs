using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Models.Template;
using Xinglin.ReportEditor.Rendering.Services;

namespace ReportDataMaker.Services.PdfExport;

public class ReportDocumentPaginator : DocumentPaginator
{
    private readonly TemplateDefinition _template;
    private readonly IPdfExportService _pdfExportService;
    private readonly Dictionary<string, object> _data;
    private readonly PdfElementRenderer _renderer = new();
    private byte[]? _pdfBytes;
    private int _pageCount;
    private readonly List<SKBitmap> _pageBitmaps = [];

    public ReportDocumentPaginator(TemplateDefinition template, IPdfExportService pdfExportService, Dictionary<string, object>? data = null)
    {
        _template = template;
        _pdfExportService = pdfExportService;
        _data = data ?? new Dictionary<string, object>();

        var widthPx = template.PageSettings.PageWidth / 25.4 * 96;
        var heightPx = template.PageSettings.PageHeight / 25.4 * 96;
        PageSize = new Size(widthPx, heightPx);

        GeneratePages();
    }

    private void GeneratePages()
    {
        try
        {
            _pdfBytes = _pdfExportService.RenderToPdf(_template, _data);
            _pageCount = GetPdfPageCount(_pdfBytes);
        }
        catch
        {
            _pageCount = 0;
        }

        if (_pageCount == 0) return;

        var layout = new PdfPageLayoutEngine(_template);
        var contentWidth = layout.PageWidth - layout.MarginLeft - layout.MarginRight;
        var contentHeight = layout.PageHeight - layout.MarginTop - layout.MarginBottom;

        var headerElements = _template.Elements
            .Where(e => e is HeaderElement)
            .OrderBy(e => e.ZIndex)
            .ToList();

        var footerElements = _template.Elements
            .Where(e => e is FooterElement)
            .OrderBy(e => e.ZIndex)
            .ToList();

        var contentElements = _template.Elements
            .Where(e => e is not HeaderElement && e is not FooterElement)
            .OrderBy(e => e.ZIndex)
            .ToList();

        for (int i = 0; i < _pageCount; i++)
        {
            using var surface = SKSurface.Create(new SKImageInfo(
                Math.Max(1, (int)layout.PageWidth),
                Math.Max(1, (int)layout.PageHeight)));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            // Render header
            if (headerElements.Count > 0)
            {
                canvas.Save();
                canvas.Translate(layout.MarginLeft, layout.MarginTop);
                foreach (var element in headerElements)
                    _renderer.RenderElement(canvas, element, _data, layout);
                canvas.Restore();
            }

            // Render content
            canvas.Save();
            canvas.Translate(layout.MarginLeft, layout.MarginTop);
            foreach (var element in contentElements)
                _renderer.RenderElement(canvas, element, _data, layout);
            canvas.Restore();

            // Render footer
            if (footerElements.Count > 0)
            {
                canvas.Save();
                canvas.Translate(layout.MarginLeft, layout.PageHeight - layout.MarginBottom);
                foreach (var element in footerElements)
                    _renderer.RenderElement(canvas, element, _data, layout);
                canvas.Restore();
            }

            using var image = surface.Snapshot();
            using var imageData = image.Encode(SKEncodedImageFormat.Png, 100);
            var bitmap = SKBitmap.Decode(imageData.ToArray());
            if (bitmap != null)
                _pageBitmaps.Add(bitmap);
        }
    }

    private static int GetPdfPageCount(byte[] pdfBytes)
    {
        try
        {
            // Count page occurrences in PDF binary content
            // Each page in a PDF has a /Type /Page entry
            var text = System.Text.Encoding.ASCII.GetString(pdfBytes);
            var count = 0;
            var idx = 0;
            while ((idx = text.IndexOf("/Type /Page", idx)) != -1)
            {
                // Make sure it's /Page and not /Pages
                if (idx + "/Type /Page".Length >= text.Length || text[idx + "/Type /Page".Length] != 's')
                    count++;
                idx++;
            }
            return Math.Max(1, count);
        }
        catch
        {
            return 1;
        }
    }

    public override bool IsPageCountValid => true;
    public override int PageCount => _pageCount;
    public override Size PageSize { get; set; }
    public override IDocumentPaginatorSource Source => null!;

    public override DocumentPage GetPage(int pageNumber)
    {
        if (pageNumber < 0 || pageNumber >= _pageCount)
            return DocumentPage.Missing;

        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            if (pageNumber < _pageBitmaps.Count)
            {
                var skBitmap = _pageBitmaps[pageNumber];

                // Convert SkiaSharp bitmap to WPF BitmapSource
                using var image = SKImage.FromBitmap(skBitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                var stream = new MemoryStream(data.ToArray());
                var bitmapSource = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

                // Calculate scale to fit PageSize
                var scaleX = PageSize.Width / skBitmap.Width;
                var scaleY = PageSize.Height / skBitmap.Height;
                var scale = Math.Min(scaleX, scaleY);

                dc.DrawImage(bitmapSource, new Rect(0, 0, skBitmap.Width * scale, skBitmap.Height * scale));
            }
            else
            {
                // Fallback: draw placeholder
                var pen = new Pen(Brushes.Gray, 1);
                dc.DrawRectangle(null, pen, new Rect(0, 0, PageSize.Width, PageSize.Height));

                var placeholder = new FormattedText(
                    $"[第 {pageNumber + 1} 页 / 共 {_pageCount} 页]",
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Microsoft YaHei"),
                    14,
                    Brushes.Gray,
                    1.0);
                dc.DrawText(placeholder, new Point(20, 20));
            }
        }

        return new DocumentPage(visual, PageSize, new Rect(PageSize), new Rect(PageSize));
    }
}

public static class PrintHelper
{
    public static void ShowPrintPreview(TemplateDefinition template, IPdfExportService pdfExportService, Dictionary<string, object>? data = null)
    {
        var paginator = new ReportDocumentPaginator(template, pdfExportService, data);
        var dialog = new System.Windows.Controls.PrintDialog();

        if (dialog.ShowDialog() == true)
        {
            dialog.PrintDocument(paginator, $"报告: {template.Name}");
        }
    }
}
