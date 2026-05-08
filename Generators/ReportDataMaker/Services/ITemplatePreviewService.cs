using System.Windows.Media;
using ReportDataMaker.Models;

namespace ReportDataMaker.Services;

public interface ITemplatePreviewService
{
    Visual GeneratePreview(ExternalTemplateDefinition template);
    byte[] RenderToImage(ExternalTemplateDefinition template, double dpi = 96);
}
