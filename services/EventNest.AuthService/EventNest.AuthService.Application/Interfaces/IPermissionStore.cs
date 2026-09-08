using EventNest.AuthService.Domain.Entities;

namespace EventNest.AuthService.Application.Interfaces;

public interface IPermissionStore
{
    Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId);
    Task AddGrantAsync(PermissionGrant grant);
    Task RemoveGrantAsync(Guid userId, string permissionName);
    Task<PermissionGrant?> GetGrantAsync(Guid userId, string permissionName);
}
