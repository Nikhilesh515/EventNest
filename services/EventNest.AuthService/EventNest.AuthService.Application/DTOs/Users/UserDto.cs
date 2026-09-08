namespace EventNest.AuthService.Application.DTOs.Users;

public record UserDto(Guid Id, string Email, string DisplayName, string RoleName, bool IsActive);
