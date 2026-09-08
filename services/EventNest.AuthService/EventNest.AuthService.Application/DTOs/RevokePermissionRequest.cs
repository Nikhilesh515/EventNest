namespace EventNest.AuthService.Application.DTOs;

public record RevokePermissionRequest(Guid UserId, string PermissionName);
