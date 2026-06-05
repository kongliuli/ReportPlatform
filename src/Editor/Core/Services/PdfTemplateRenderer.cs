using Xinglin.ReportEditor.Contracts.Models.Template;
using Xinglin.ReportEditor.Core.SharedInterfaces;
using Xinglin.ReportEditor.Rendering.Services;

namespace Xinglin.ReportEditor.Core.Services;

public class PdfTemplateRenderer : IPdfSharpTemplateRenderer
{
    private readonly TemplateRenderer _renderer = new();

    public byte[] RenderToPdf(string templateJson) => _renderer.RenderToPdf(templateJson);
    public byte[] RenderToPdf(TemplateDefinition template) => _renderer.RenderToPdf(template);
    public byte[] RenderToImage(string templateJson) => _renderer.RenderToImage(templateJson);
}
