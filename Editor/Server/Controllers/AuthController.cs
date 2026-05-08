using Microsoft.AspNetCore.Mvc;
using Xinglin.WebReportEditor.Contracts.DTOs;
using Xinglin.WebReportEditor.Contracts.Responses;
using Xinglin.WebReportEditor.Core.Services;

namespace Xinglin.WebReportEditor.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ApiResponse<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return ApiResponse<LoginResponse>.Ok(result, "登录成功");
        }
        catch (UnauthorizedAccessException ex)
        {
            return ApiResponse<LoginResponse>.Fail(ex.Message, 401);
        }
    }

    [HttpPost("refresh")]
    public async Task<ApiResponse<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            return ApiResponse<RefreshTokenResponse>.Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ApiResponse<RefreshTokenResponse>.Fail(ex.Message, 401);
        }
    }

    [HttpPost("logout")]
    public async Task<ApiResponse<object>> Logout([FromBody] RefreshTokenRequest? request)
    {
        if (request?.RefreshToken != null)
        {
            await _authService.RevokeTokenAsync(request.RefreshToken);
        }
        return ApiResponse<object>.Ok(null!, "登出成功");
    }
}
