using Microsoft.AspNetCore.Mvc;
using Xinglin.WebReportEditor.Contracts.Responses;
using Xinglin.WebReportEditor.Core.Services;

namespace Xinglin.WebReportEditor.Server.Controllers;

[ApiController]
[Route("api/templates/{templateId:guid}/preview")]
public class PreviewController : ControllerBase
{
    private readonly IPdfRenderService _pdfRenderService;
    private readonly ILogger<PreviewController> _logger;

    public PreviewController(IPdfRenderService pdfRenderService, ILogger<PreviewController> logger)
    {
        _pdfRenderService = pdfRenderService;
        _logger = logger;
    }

    [HttpGet("image")]
    public async Task<ApiResponse<string>> GetPreviewImage(Guid templateId)
    {
        try
        {
            var base64Image = await _pdfRenderService.RenderToImageAsync(templateId);
            return ApiResponse<string>.Ok(base64Image);
        }
        catch (KeyNotFoundException ex)
        {
            return ApiResponse<string>.Fail(ex.Message, 404);
        }
    }

    [HttpGet("pdf")]
    public async Task<IActionResult> GetPdf(Guid templateId)
    {
        try
        {
            var pdfBytes = await _pdfRenderService.RenderToPdfAsync(templateId);
            if (pdfBytes.Length == 0)
            {
                return NotFound("PDF 渲染服务待配置");
            }

            return File(pdfBytes, "application/pdf", $"template_{templateId}.pdf");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
