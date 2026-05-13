using ReportDataMaker.Models;

namespace ReportDataMaker.Services.PdfExport;

public class PdfPageLayoutEngine
{
    public float PageWidth { get; }
    public float PageHeight { get; }
    public float MarginLeft { get; }
    public float MarginRight { get; }
    public float MarginTop { get; }
    public float MarginBottom { get; }
    public bool IsLandscape { get; }
    private readonly float _scale;

    private const float MmToPoints = 2.835f;

    public PdfPageLayoutEngine(ExternalTemplateDefinition template) : this(template, 1.0f) { }

    public PdfPageLayoutEngine(ExternalTemplateDefinition template, double scale)
    {
        _scale = (float)scale;
        IsLandscape = template.Orientation?.Equals("Landscape", StringComparison.OrdinalIgnoreCase) == true;

        var pageWidth = (float)(template.PageWidth * MmToPoints * scale);
        var pageHeight = (float)(template.PageHeight * MmToPoints * scale);

        if (IsLandscape)
            (pageWidth, pageHeight) = (pageHeight, pageWidth);

        PageWidth = pageWidth;
        PageHeight = pageHeight;
        MarginLeft = (float)(template.MarginLeft * MmToPoints * scale);
        MarginRight = (float)(template.MarginRight * MmToPoints * scale);
        MarginTop = (float)(template.MarginTop * MmToPoints * scale);
        MarginBottom = (float)(template.MarginBottom * MmToPoints * scale);
    }

    public float ConvertX(double x) => (float)(x * MmToPoints * _scale);
    public float ConvertY(double y) => (float)(y * MmToPoints * _scale);
    public float ConvertSize(double size) => (float)(size * MmToPoints * _scale);
}
