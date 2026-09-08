using EventNest.AuthService.Application.DTOs.Permissions;

namespace EventNest.AuthService.Application.Services.Interfaces;

public interface IPermissionService
{
    Task<IReadOnlyList<PermissionDto>> GetUserPermissionsAsync(Guid userId);
    Task<PermissionDto> GrantAsync(Guid userId, string permissionName, DateTime? expiresAt);
    Task RevokeAsync(Guid userId, string permissionName);
    Task<bool> CheckAsync(Guid userId, string permissionName);
}
