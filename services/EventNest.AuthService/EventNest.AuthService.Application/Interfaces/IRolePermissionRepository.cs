using EventNest.AuthService.Domain.Entities;

namespace EventNest.AuthService.Application.Interfaces;

public interface IRolePermissionRepository
{
    Task<IReadOnlyList<RolePermission>> GetByRoleIdAsync(Guid roleId);
    Task AddRangeAsync(IEnumerable<RolePermission> permissions);
    Task RemoveByRoleIdAsync(Guid roleId);
}
