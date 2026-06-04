using System.Windows.Media;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Services;

public interface ITemplatePreviewService
{
    Visual GeneratePreview(TemplateDefinition template);
    byte[] RenderToImage(TemplateDefinition template, double dpi = 96);
}
