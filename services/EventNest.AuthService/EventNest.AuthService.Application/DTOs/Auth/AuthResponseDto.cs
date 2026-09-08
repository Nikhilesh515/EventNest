using EventNest.AuthService.Application.DTOs.Users;

namespace EventNest.AuthService.Application.DTOs.Auth;

public record AuthResponseDto(string AccessToken, string RefreshToken, int ExpiresIn, UserDto User);
