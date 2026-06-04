using Microsoft.AspNetCore.Mvc;
using Xinglin.WebReportEditor.Contracts.DTOs;
using Xinglin.WebReportEditor.Contracts.Requests;
using Xinglin.WebReportEditor.Contracts.Responses;
using Xinglin.ReportEditor.Core.Services;

namespace Xinglin.ReportEditor.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly ILogger<TemplatesController> _logger;

    public TemplatesController(ITemplateService templateService, ILogger<TemplatesController> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ApiResponse<PagedResponse<TemplateDto>>> GetTemplates([FromQuery] TemplateFilterRequest filter)
    {
        var result = await _templateService.GetTemplatesAsync(filter);
        return ApiResponse<PagedResponse<TemplateDto>>.Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ApiResponse<TemplateDetailDto>> GetTemplate(Guid id)
    {
        var result = await _templateService.GetTemplateAsync(id);
        if (result == null)
            return ApiResponse<TemplateDetailDto>.Fail("模板不存在", 404);

        return ApiResponse<TemplateDetailDto>.Ok(result);
    }

    [HttpPost]
    public async Task<ApiResponse<TemplateDetailDto>> CreateTemplate([FromBody] CreateTemplateRequest request)
    {
        var result = await _templateService.CreateTemplateAsync(request);
        return ApiResponse<TemplateDetailDto>.Ok(result, "创建成功");
    }

    [HttpPut("{id:guid}")]
    public async Task<ApiResponse<TemplateDetailDto>> UpdateTemplate(Guid id, [FromBody] UpdateTemplateRequest request)
    {
        var result = await _templateService.UpdateTemplateAsync(id, request);
        if (result == null)
            return ApiResponse<TemplateDetailDto>.Fail("模板不存在", 404);

        return ApiResponse<TemplateDetailDto>.Ok(result, "更新成功");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ApiResponse<bool>> DeleteTemplate(Guid id)
    {
        var result = await _templateService.DeleteTemplateAsync(id);
        if (!result)
            return ApiResponse<bool>.Fail("模板不存在", 404);

        return ApiResponse<bool>.Ok(true, "删除成功");
    }
}
