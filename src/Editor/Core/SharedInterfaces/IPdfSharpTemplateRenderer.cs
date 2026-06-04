using Xinglin.ReportEditor.Contracts.Models.Template;

namespace Xinglin.ReportEditor.Core.SharedInterfaces;

[Obsolete("后续由 Rendering 项目的 ITemplateRenderer 替代")]
public interface IPdfSharpTemplateRenderer
{
    /// <summary>Render template JSON to PDF bytes</summary>
    byte[] RenderToPdf(string templateJson);

    /// <summary>Render template definition to PDF bytes</summary>
    byte[] RenderToPdf(TemplateDefinition template);
}
