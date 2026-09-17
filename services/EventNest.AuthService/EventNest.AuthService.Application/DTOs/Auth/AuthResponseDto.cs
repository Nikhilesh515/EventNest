using System.Text.Json.Serialization;
using EventNest.AuthService.Application.DTOs.Users;

namespace EventNest.AuthService.Application.DTOs.Auth;

public record AuthResponseDto(string AccessToken, int ExpiresIn, UserDto User)
{
    [JsonIgnore] public string RefreshTokenValue { get; init; } = string.Empty;
}
