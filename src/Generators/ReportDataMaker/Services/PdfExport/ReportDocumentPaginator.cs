using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Services.PdfExport;

public class ReportDocumentPaginator : DocumentPaginator
{
    private readonly TemplateDefinition _template;
    private readonly Dictionary<string, object> _data;
    private readonly IPdfExportService _pdfExportService;
    private readonly byte[]? _pdfBytes;

    public ReportDocumentPaginator(TemplateDefinition template, Dictionary<string, object> data, IPdfExportService pdfExportService)
    {
        _template = template;
        _data = data;
        _pdfExportService = pdfExportService;

        var widthPx = template.PageSettings.PageWidth / 25.4 * 96;
        var heightPx = template.PageSettings.PageHeight / 25.4 * 96;
        PageSize = new Size(widthPx, heightPx);

        try { _pdfBytes = _pdfExportService.RenderToPdf(template, data); }
        catch { _pdfBytes = null; }
    }

    public override bool IsPageCountValid => true;
    public override int PageCount => 1;
    public override Size PageSize { get; set; }
    public override IDocumentPaginatorSource Source => null!;

    public override DocumentPage GetPage(int pageNumber)
    {
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            var pen = new Pen(Brushes.Gray, 1);
            dc.DrawRectangle(null, pen, new Rect(0, 0, PageSize.Width, PageSize.Height));

            var text = new FormattedText(
                _template.Name,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Microsoft YaHei"),
                14,
                Brushes.Black,
                1.0);
            dc.DrawText(text, new Point(20, 20));

            var placeholder = new FormattedText(
                $"[PDF 预览 - {_template.Name}]",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Microsoft YaHei"),
                12,
                Brushes.Gray,
                1.0);
            dc.DrawText(placeholder, new Point(20, 50));

            var info = new FormattedText(
                $"元素数量: {_template.Elements?.Count ?? 0}",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Microsoft YaHei"),
                10,
                Brushes.Gray,
                1.0);
            dc.DrawText(info, new Point(20, 80));
        }

        return new DocumentPage(visual, PageSize, new Rect(PageSize), new Rect(PageSize));
    }
}

public static class PrintHelper
{
    public static void ShowPrintPreview(TemplateDefinition template, Dictionary<string, object> data, IPdfExportService pdfExportService)
    {
        var paginator = new ReportDocumentPaginator(template, data, pdfExportService);
        var dialog = new System.Windows.Controls.PrintDialog();

        if (dialog.ShowDialog() == true)
        {
            dialog.PrintDocument(paginator, $"报告: {template.Name}");
        }
    }
}
