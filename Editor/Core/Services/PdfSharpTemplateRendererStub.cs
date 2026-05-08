using Xinglin.WebReportEditor.Core.SharedInterfaces;

namespace Xinglin.WebReportEditor.Core.Services;

public class PdfSharpTemplateRendererStub : IPdfSharpTemplateRenderer
{
    public byte[] RenderToPdf(object templateDefinition)
    {
        return Array.Empty<byte>();
    }
}
