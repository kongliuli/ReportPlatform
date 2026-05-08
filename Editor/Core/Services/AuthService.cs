using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Xinglin.WebReportEditor.Contracts.DTOs;
using Xinglin.WebReportEditor.Core.Data;

namespace Xinglin.WebReportEditor.Core.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken);
    Task RevokeTokenAsync(string refreshToken);
    Task<UserDto?> GetUserByCredentialsAsync(string username, string password);
}

public class AuthService : IAuthService
{
    private readonly TemplateDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthService(TemplateDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("用户名或密码错误");
        }

        var accessToken = GenerateAccessToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = MapToUserDto(user)
        };
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await _dbContext.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (tokenEntity == null || tokenEntity.IsRevoked || tokenEntity.ExpiryTime < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("无效或已过期的刷新令牌");
        }

        tokenEntity.IsRevoked = true;
        _dbContext.RefreshTokens.Update(tokenEntity);

        var newAccessToken = GenerateAccessToken(tokenEntity.User);
        var newRefreshToken = await GenerateRefreshTokenAsync(tokenEntity.UserId);

        await _dbContext.SaveChangesAsync();

        return new RefreshTokenResponse
        {
            AccessToken = newAccessToken
        };
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        var tokenEntity = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (tokenEntity != null)
        {
            tokenEntity.IsRevoked = true;
            _dbContext.RefreshTokens.Update(tokenEntity);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<UserDto?> GetUserByCredentialsAsync(string username, string password)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return null;
        }

        return MapToUserDto(user);
    }

    private string GenerateAccessToken(UserEntity user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("HospitalId", user.HospitalId ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var expirationMinutes = jwtSettings.GetValue<int>("AccessTokenExpirationMinutes", 30);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> GenerateRefreshTokenAsync(Guid userId)
    {
        var expirationDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7);
        var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

        var entity = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = userId,
            ExpiryTime = DateTime.UtcNow.AddDays(expirationDays),
            IsRevoked = false,
            CreateTime = DateTime.UtcNow
        };

        _dbContext.RefreshTokens.Add(entity);
        await _dbContext.SaveChangesAsync();

        return token;
    }

    private static UserDto MapToUserDto(UserEntity user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Role = user.Role,
            HospitalId = user.HospitalId
        };
    }
}
