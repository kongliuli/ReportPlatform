using Xinglin.ReportEditor.Core.Data;
using Xinglin.ReportEditor.Core.SharedInterfaces;

namespace Xinglin.ReportEditor.Core.Services;

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

        return _renderer.RenderToPdf(template.ContentJson);
    }

    public async Task<string> RenderToImageAsync(Guid templateId)
    {
        var template = await _dbContext.Templates.FindAsync(templateId)
            ?? throw new KeyNotFoundException($"模板 {templateId} 不存在");

        var imageBytes = _renderer.RenderToImage(template.ContentJson);
        return Convert.ToBase64String(imageBytes);
    }
}
