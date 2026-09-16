using EventNest.Shared.Domain.Entities;

namespace EventNest.AuthService.Domain.Entities;

public class RolePermission : BaseEntity
{
    public Guid RoleId { get; private set; }
    public Role Role { get; private set; } = null!;
    public string PermissionName { get; private set; } = string.Empty;

    private RolePermission() { }

    public static RolePermission Create(Guid roleId, string permissionName)
    {
        return new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionName = permissionName,
            CreatedAt = DateTime.UtcNow
        };
    }
}
