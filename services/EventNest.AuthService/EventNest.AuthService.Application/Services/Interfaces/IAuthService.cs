using EventNest.AuthService.Application.DTOs.Auth;

namespace EventNest.AuthService.Application.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(string email, string displayName, string password, string? ipAddress);
    Task<AuthResponseDto> LoginAsync(string email, string password, string? ipAddress);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string? ipAddress);
    Task LogoutAsync(string refreshToken, string? ipAddress);
}
