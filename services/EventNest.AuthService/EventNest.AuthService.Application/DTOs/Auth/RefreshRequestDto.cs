namespace EventNest.AuthService.Application.DTOs.Auth;

public class RefreshRequestDto
{
    public string RefreshToken { get; init; } = string.Empty;
}
