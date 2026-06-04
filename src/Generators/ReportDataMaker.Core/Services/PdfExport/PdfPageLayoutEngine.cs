using Xinglin.ReportEditor.Contracts.Models.Template;

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

    public PdfPageLayoutEngine(TemplateDefinition template) : this(template, 1.0f) { }

    public PdfPageLayoutEngine(TemplateDefinition template, double scale)
    {
        _scale = (float)scale;
        IsLandscape = template.PageSettings.Orientation == PageOrientation.Landscape;

        var pageWidth = (float)(template.PageSettings.PageWidth * MmToPoints * scale);
        var pageHeight = (float)(template.PageSettings.PageHeight * MmToPoints * scale);

        if (IsLandscape)
            (pageWidth, pageHeight) = (pageHeight, pageWidth);

        PageWidth = pageWidth;
        PageHeight = pageHeight;
        MarginLeft = (float)(template.PageSettings.MarginLeft * MmToPoints * scale);
        MarginRight = (float)(template.PageSettings.MarginRight * MmToPoints * scale);
        MarginTop = (float)(template.PageSettings.MarginTop * MmToPoints * scale);
        MarginBottom = (float)(template.PageSettings.MarginBottom * MmToPoints * scale);
    }

    public float ConvertX(double x) => (float)(x * MmToPoints * _scale);
    public float ConvertY(double y) => (float)(y * MmToPoints * _scale);
    public float ConvertSize(double size) => (float)(size * MmToPoints * _scale);
}
