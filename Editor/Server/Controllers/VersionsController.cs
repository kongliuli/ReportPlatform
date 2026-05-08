using Microsoft.AspNetCore.Mvc;
using Xinglin.WebReportEditor.Contracts.DTOs;
using Xinglin.WebReportEditor.Contracts.Responses;
using Xinglin.WebReportEditor.Core.Services;

namespace Xinglin.WebReportEditor.Server.Controllers;

[ApiController]
[Route("api/templates/{templateId:guid}/versions")]
public class VersionsController : ControllerBase
{
    private readonly IVersionService _versionService;
    private readonly ILogger<VersionsController> _logger;

    public VersionsController(IVersionService versionService, ILogger<VersionsController> logger)
    {
        _versionService = versionService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ApiResponse<List<TemplateVersionDto>>> GetVersions(Guid templateId)
    {
        var result = await _versionService.GetVersionsAsync(templateId);
        return ApiResponse<List<TemplateVersionDto>>.Ok(result);
    }

    [HttpGet("{versionId:guid}")]
    public async Task<ApiResponse<TemplateVersionDetailDto>> GetVersion(Guid templateId, Guid versionId)
    {
        var result = await _versionService.GetVersionAsync(templateId, versionId);
        if (result == null)
            return ApiResponse<TemplateVersionDetailDto>.Fail("版本不存在", 404);

        return ApiResponse<TemplateVersionDetailDto>.Ok(result);
    }

    [HttpPost("{versionId:guid}/rollback")]
    public async Task<ApiResponse<TemplateVersionDto>> Rollback(Guid templateId, Guid versionId, [FromBody] RollbackRequest? request)
    {
        try
        {
            var result = await _versionService.RollbackAsync(templateId, versionId, request?.CreatedBy);
            return ApiResponse<TemplateVersionDto>.Ok(result, "回滚成功");
        }
        catch (KeyNotFoundException ex)
        {
            return ApiResponse<TemplateVersionDto>.Fail(ex.Message, 404);
        }
    }

    [HttpGet("diff")]
    public async Task<ApiResponse<VersionDiffResponse>> Diff(Guid templateId, [FromQuery] Guid vidA, [FromQuery] Guid vidB)
    {
        try
        {
            var result = await _versionService.DiffAsync(templateId, vidA, vidB);
            return ApiResponse<VersionDiffResponse>.Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return ApiResponse<VersionDiffResponse>.Fail(ex.Message, 404);
        }
    }
}
