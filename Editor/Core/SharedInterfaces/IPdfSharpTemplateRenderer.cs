using Xinglin.ReportEditor.Contracts.Models.Template;

namespace Xinglin.WebReportEditor.Core.SharedInterfaces;

public interface IPdfSharpTemplateRenderer
{
    /// <summary>Render template JSON to PDF bytes</summary>
    byte[] RenderToPdf(string templateJson);

    /// <summary>Render template definition to PDF bytes</summary>
    byte[] RenderToPdf(TemplateDefinition template);
}
