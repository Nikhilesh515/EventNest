namespace EventNest.AuthService.Application.DTOs.Permissions;

public record GrantPermissionRequestDto(Guid UserId, string PermissionName, DateTime? ExpiresAt);
