using Xinglin.ReportEditor.Contracts.Models.Template;

namespace Xinglin.ReportEditor.Core.SharedInterfaces;

public interface IPdfSharpTemplateRenderer
{
    /// <summary>Render template JSON to PDF bytes</summary>
    byte[] RenderToPdf(string templateJson);

    /// <summary>Render template definition to PDF bytes</summary>
    byte[] RenderToPdf(TemplateDefinition template);

    /// <summary>Render first page of template JSON to PNG image bytes</summary>
    byte[] RenderToImage(string templateJson);
}
