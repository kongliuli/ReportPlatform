using Xinglin.WebReportEditor.Contracts.DTOs;

namespace Xinglin.ReportEditor.Contracts.Abstractions;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken);
    Task RevokeTokenAsync(string refreshToken);
    Task<UserDto?> GetUserByCredentialsAsync(string username, string password);
}
