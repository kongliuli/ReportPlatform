using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xinglin.WebReportEditor.Contracts.DTOs;
using Xinglin.WebReportEditor.Contracts.Responses;
using Xinglin.WebReportEditor.Core.Services;
using Xinglin.WebReportEditor.Server.Controllers;

namespace Xinglin.WebReportEditor.Server.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_authServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        var request = new LoginRequest { Username = "admin", Password = "admin123" };
        var expectedResponse = new LoginResponse
        {
            AccessToken = "test-token",
            RefreshToken = "test-refresh",
            User = new UserDto { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Username = "admin", DisplayName = "Admin", Role = "admin" }
        };
        _authServiceMock.Setup(x => x.LoginAsync(request)).ReturnsAsync(expectedResponse);

        var result = await _controller.Login(request);

        var apiResult = Assert.IsType<ApiResponse<LoginResponse>>(result);
        Assert.Equal(200, apiResult.Code);
        Assert.NotNull(apiResult.Data);
        Assert.Equal("test-token", apiResult!.Data!.AccessToken);
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401()
    {
        var request = new LoginRequest { Username = "admin", Password = "wrong" };
        _authServiceMock.Setup(x => x.LoginAsync(request))
            .ThrowsAsync(new UnauthorizedAccessException("用户名或密码错误"));

        var result = await _controller.Login(request);

        var apiResult = Assert.IsType<ApiResponse<LoginResponse>>(result);
        Assert.NotEqual(200, apiResult.Code);
        Assert.Equal(401, apiResult.Code);
    }

    [Fact]
    public async Task RefreshToken_ValidToken_ReturnsOk()
    {
        var request = new RefreshTokenRequest { RefreshToken = "valid-refresh-token" };
        var expected = new RefreshTokenResponse
        {
            AccessToken = "new-token"
        };
        _authServiceMock.Setup(x => x.RefreshTokenAsync("valid-refresh-token")).ReturnsAsync(expected);

        var result = await _controller.RefreshToken(request);

        var apiResult = Assert.IsType<ApiResponse<RefreshTokenResponse>>(result);
        Assert.Equal(200, apiResult.Code);
        Assert.Equal("new-token", apiResult.Data.AccessToken);
    }

    [Fact]
    public async Task RefreshToken_InvalidToken_Returns401()
    {
        var request = new RefreshTokenRequest { RefreshToken = "invalid" };
        _authServiceMock.Setup(x => x.RefreshTokenAsync("invalid"))
            .ThrowsAsync(new UnauthorizedAccessException("刷新令牌无效"));

        var result = await _controller.RefreshToken(request);

        var apiResult = Assert.IsType<ApiResponse<RefreshTokenResponse>>(result);
        Assert.NotEqual(200, apiResult.Code);
        Assert.Equal(401, apiResult.Code);
    }

    [Fact]
    public async Task Logout_WithToken_RevokesToken()
    {
        var request = new RefreshTokenRequest { RefreshToken = "token-to-revoke" };
        _authServiceMock.Setup(x => x.RevokeTokenAsync("token-to-revoke")).Returns(Task.CompletedTask);

        var result = await _controller.Logout(request);

        var apiResult = Assert.IsType<ApiResponse<object>>(result);
        Assert.Equal(200, apiResult.Code);
        _authServiceMock.Verify(x => x.RevokeTokenAsync("token-to-revoke"), Times.Once);
    }
}

