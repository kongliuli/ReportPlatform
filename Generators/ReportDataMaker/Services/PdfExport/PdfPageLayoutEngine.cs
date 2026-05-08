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

    private const float MmToPoints = 2.835f;

    public PdfPageLayoutEngine(ExternalTemplateDefinition template)
    {
        IsLandscape = template.Orientation?.Equals("Landscape", StringComparison.OrdinalIgnoreCase) == true;

        var pageWidth = (float)template.PageWidth * MmToPoints;
        var pageHeight = (float)template.PageHeight * MmToPoints;

        if (IsLandscape)
            (pageWidth, pageHeight) = (pageHeight, pageWidth);

        PageWidth = pageWidth;
        PageHeight = pageHeight;
        MarginLeft = (float)template.MarginLeft * MmToPoints;
        MarginRight = (float)template.MarginRight * MmToPoints;
        MarginTop = (float)template.MarginTop * MmToPoints;
        MarginBottom = (float)template.MarginBottom * MmToPoints;
    }

    public float ConvertX(double x) => (float)(x * MmToPoints);
    public float ConvertY(double y) => (float)(y * MmToPoints);
    public float ConvertSize(double size) => (float)(size * MmToPoints);
}
