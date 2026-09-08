namespace EventNest.AuthService.Application.DTOs.Auth;

public record RegisterRequestDto(string Email, string DisplayName, string Password);
