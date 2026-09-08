namespace EventNest.AuthService.Application.DTOs;

public record GrantPermissionRequest(Guid UserId, string PermissionName, DateTime? ExpiresAt);
