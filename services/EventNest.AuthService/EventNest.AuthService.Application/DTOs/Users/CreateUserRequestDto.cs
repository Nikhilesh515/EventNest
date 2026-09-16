namespace EventNest.AuthService.Application.DTOs.Users;

public record CreateUserRequestDto(string Email, string DisplayName, string Password, Guid RoleId);
