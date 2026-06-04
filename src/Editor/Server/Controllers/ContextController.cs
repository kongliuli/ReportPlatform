using Microsoft.AspNetCore.Mvc;
using Xinglin.WebReportEditor.Contracts.Responses;
using Xinglin.ReportEditor.Core.Services;

namespace Xinglin.ReportEditor.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContextController : ControllerBase
{
    private readonly ContextService _contextService;

    public ContextController(ContextService contextService)
    {
        _contextService = contextService;
    }

    /// <summary>
    /// 获取所有内置上下文值
    /// </summary>
    [HttpGet("values")]
    public ApiResponse<Dictionary<string, object>> GetContextValues()
    {
        var values = _contextService.GetAllBuiltInValues();
        return ApiResponse<Dictionary<string, object>>.Ok(values);
    }

    /// <summary>
    /// 获取指定路径的上下文值
    /// </summary>
    [HttpGet("resolve")]
    public ApiResponse<object?> ResolveContextValue([FromQuery] string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return ApiResponse<object?>.Fail("路径参数不能为空", 400);

        var value = _contextService.GetContextValue(path);
        if (value == null)
            return ApiResponse<object?>.Fail($"未识别的上下文路径: {path}", 404);

        return ApiResponse<object?>.Ok(value);
    }
}
