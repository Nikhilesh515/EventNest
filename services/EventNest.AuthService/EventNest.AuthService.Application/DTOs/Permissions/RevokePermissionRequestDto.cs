namespace EventNest.AuthService.Application.DTOs.Permissions;

public record RevokePermissionRequestDto(Guid UserId, string PermissionName);
