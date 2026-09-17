namespace EventNest.AuthService.Application.DTOs.Auth;

public class LogoutRequestDto
{
    public string RefreshToken { get; init; } = string.Empty;
}
