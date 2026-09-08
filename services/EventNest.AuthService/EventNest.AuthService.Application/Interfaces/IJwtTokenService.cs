using EventNest.AuthService.Domain.Entities;

namespace EventNest.AuthService.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, IReadOnlyList<string> permissions);
    string GenerateRefreshToken();
    string? ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
}
