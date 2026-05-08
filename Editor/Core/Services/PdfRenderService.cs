using Xinglin.WebReportEditor.Core.Data;
using Xinglin.WebReportEditor.Core.SharedInterfaces;

namespace Xinglin.WebReportEditor.Core.Services;

public interface IPdfRenderService
{
    Task<byte[]> RenderToPdfAsync(Guid templateId);
    Task<string> RenderToImageAsync(Guid templateId);
}

public class PdfRenderService : IPdfRenderService
{
    private readonly TemplateDbContext _dbContext;
    private readonly IPdfSharpTemplateRenderer _renderer;

    public PdfRenderService(TemplateDbContext dbContext, IPdfSharpTemplateRenderer renderer)
    {
        _dbContext = dbContext;
        _renderer = renderer;
    }

    public async Task<byte[]> RenderToPdfAsync(Guid templateId)
    {
        var template = await _dbContext.Templates.FindAsync(templateId)
            ?? throw new KeyNotFoundException($"模板 {templateId} 不存在");

        var pdfBytes = _renderer.RenderToPdf(template.ContentJson);
        return pdfBytes;
    }

    public async Task<string> RenderToImageAsync(Guid templateId)
    {
        var template = await _dbContext.Templates.FindAsync(templateId)
            ?? throw new KeyNotFoundException($"模板 {templateId} 不存在");

        var pdfBytes = _renderer.RenderToPdf(template.ContentJson);

        if (pdfBytes == null || pdfBytes.Length == 0)
        {
            return GeneratePlaceholderImage(template.ContentJson);
        }

        return Convert.ToBase64String(pdfBytes);
    }

    private static string GeneratePlaceholderImage(string contentJson)
    {
        var placeholderSvg = $@"
<svg xmlns='http://www.w3.org/2000/svg' width='794' height='1123' viewBox='0 0 794 1123'>
  <rect width='794' height='1123' fill='white' stroke='#ccc' stroke-width='1'/>
  <text x='397' y='561' text-anchor='middle' font-size='24' fill='#999'>预览占位图</text>
  <text x='397' y='591' text-anchor='middle' font-size='14' fill='#bbb'>PDF 渲染服务待配置</text>
</svg>";
        var svgBytes = System.Text.Encoding.UTF8.GetBytes(placeholderSvg);
        return Convert.ToBase64String(svgBytes);
    }
}
